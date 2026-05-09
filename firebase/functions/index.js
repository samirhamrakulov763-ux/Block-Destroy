const functions = require('firebase-functions');
const admin = require('firebase-admin');
const {google} = require('googleapis');

admin.initializeApp();

/**
 * Validate receipt from Google Play or App Store
 * This function provides server-side validation for in-app purchases
 */
exports.validateReceipt = functions.https.onRequest(async (req, res) => {
  // Enable CORS
  res.set('Access-Control-Allow-Origin', '*');
  res.set('Access-Control-Allow-Methods', 'POST');
  res.set('Access-Control-Allow-Headers', 'Content-Type');

  if (req.method === 'OPTIONS') {
    res.status(204).send('');
    return;
  }

  if (req.method !== 'POST') {
    res.status(405).send('Method Not Allowed');
    return;
  }

  try {
    const {productId, receipt, platform, userId, timestamp} = req.body;

    if (!productId || !receipt || !platform) {
      res.status(400).json({
        valid: false,
        message: 'Missing required fields'
      });
      return;
    }

    console.log(`Validating receipt for ${productId} on ${platform}`);

    let validationResult;

    if (platform.includes('Android')) {
      validationResult = await validateGooglePlayReceipt(receipt, productId);
    } else if (platform.includes('iOS')) {
      validationResult = await validateAppStoreReceipt(receipt);
    } else {
      res.status(400).json({
        valid: false,
        message: 'Unsupported platform'
      });
      return;
    }

    if (validationResult.valid) {
      // Log successful purchase to Firestore
      await admin.firestore().collection('purchases').add({
        userId: userId,
        productId: productId,
        platform: platform,
        timestamp: timestamp,
        transactionId: validationResult.transactionId,
        validatedAt: admin.firestore.FieldValue.serverTimestamp()
      });

      res.status(200).json({
        valid: true,
        message: 'Receipt validated successfully',
        transactionId: validationResult.transactionId
      });
    } else {
      res.status(200).json({
        valid: false,
        message: validationResult.message || 'Receipt validation failed'
      });
    }

  } catch (error) {
    console.error('Error validating receipt:', error);
    res.status(500).json({
      valid: false,
      message: 'Internal server error'
    });
  }
});

/**
 * Validate Google Play receipt
 */
async function validateGooglePlayReceipt(receipt, productId) {
  try {
    const receiptData = JSON.parse(receipt);
    const packageName = 'com.azdilgroup.blockdestroy'; // Your package name
    const purchaseToken = receiptData.purchaseToken;

    // Initialize Google Play Developer API
    const auth = new google.auth.GoogleAuth({
      keyFile: './service-account-key.json', // You'll need to add this
      scopes: ['https://www.googleapis.com/auth/androidpublisher']
    });

    const androidPublisher = google.androidpublisher({
      version: 'v3',
      auth: auth
    });

    // Verify the purchase
    const result = await androidPublisher.purchases.products.get({
      packageName: packageName,
      productId: productId,
      token: purchaseToken
    });

    const purchaseState = result.data.purchaseState;

    // purchaseState: 0 = purchased, 1 = canceled, 2 = pending
    if (purchaseState === 0) {
      return {
        valid: true,
        transactionId: result.data.orderId
      };
    } else {
      return {
        valid: false,
        message: `Invalid purchase state: ${purchaseState}`
      };
    }

  } catch (error) {
    console.error('Google Play validation error:', error);
    return {
      valid: false,
      message: error.message
    };
  }
}

/**
 * Validate App Store receipt
 */
async function validateAppStoreReceipt(receipt) {
  try {
    const https = require('https');

    // Use production URL, fallback to sandbox if needed
    const verifyUrl = 'https://buy.itunes.apple.com/verifyReceipt';
    const sandboxUrl = 'https://sandbox.itunes.apple.com/verifyReceipt';

    const verifyReceipt = (url) => {
      return new Promise((resolve, reject) => {
        const postData = JSON.stringify({
          'receipt-data': receipt,
          'password': 'YOUR_APP_SHARED_SECRET', // Get from App Store Connect
          'exclude-old-transactions': true
        });

        const options = {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Content-Length': Buffer.byteLength(postData)
          }
        };

        const req = https.request(url, options, (res) => {
          let data = '';
          res.on('data', (chunk) => data += chunk);
          res.on('end', () => resolve(JSON.parse(data)));
        });

        req.on('error', reject);
        req.write(postData);
        req.end();
      });
    };

    let result = await verifyReceipt(verifyUrl);

    // If status is 21007, receipt is from sandbox
    if (result.status === 21007) {
      result = await verifyReceipt(sandboxUrl);
    }

    if (result.status === 0) {
      const latestReceipt = result.latest_receipt_info?.[0] || result.receipt?.in_app?.[0];

      return {
        valid: true,
        transactionId: latestReceipt?.transaction_id
      };
    } else {
      return {
        valid: false,
        message: `App Store validation failed with status: ${result.status}`
      };
    }

  } catch (error) {
    console.error('App Store validation error:', error);
    return {
      valid: false,
      message: error.message
    };
  }
}

/**
 * Get purchase history for a user
 */
exports.getPurchaseHistory = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const userId = data.userId || context.auth.uid;

  try {
    const snapshot = await admin.firestore()
      .collection('purchases')
      .where('userId', '==', userId)
      .orderBy('validatedAt', 'desc')
      .limit(50)
      .get();

    const purchases = [];
    snapshot.forEach(doc => {
      purchases.push({
        id: doc.id,
        ...doc.data()
      });
    });

    return {purchases};
  } catch (error) {
    console.error('Error fetching purchase history:', error);
    throw new functions.https.HttpsError('internal', 'Failed to fetch purchase history');
  }
});
