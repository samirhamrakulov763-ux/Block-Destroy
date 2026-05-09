# Настройка AdMob и IAP для Block Destroy

## ✅ ВАЖНО: Настройка AdMob в Unity Editor

### Шаг 1: Настройте App ID через меню Unity
1. Откройте Unity Editor
2. Перейдите в **Assets → Google Mobile Ads → Settings**
3. Введите ваш Android App ID: `ca-app-pub-4928575929411783~3365414682`
4. Сохраните настройки

Это автоматически добавит App ID в AndroidManifest.xml

---

## 📱 AdMob - Текущие настройки

### Рекламные блоки (Production):
- **Banner**: ca-app-pub-4928575929411783/6435033796
- **Interstitial**: ca-app-pub-4928575929411783/9811606792
- **Rewarded**: ca-app-pub-4928575929411783/2847314012

### Как работает реклама:
- **Banner**: Показывается на всех экранах (Home, Play, Result)
- **Interstitial**: Каждые 5 завершенных игр
- **Rewarded**: В магазине для бесплатных наград

---

## 💰 Unity IAP - Настройка

### Продукты в Google Play Console:
- gem__30 ($0.99) - 30 Gems
- gem__80 ($1.99) - 80 Gems
- gem_170 ($3.99) - 170 Gems
- gem_360 ($7.99) - 360 Gems
- gem_950 ($19.99) - 950 Gems
- gem_2000 ($39.99) - 2000 Gems

### Как работает IAP:
- В редакторе: Показывает окно покупки Unity (Fake Store)
- На устройстве: Реальная покупка через Google Play

---

## 🧪 Тестирование

### AdMob:
1. **В редакторе**: Реклама НЕ показывается (это нормально)
2. **На устройстве**: 
   - Первый запуск: Подождите 5-10 минут для кэширования рекламы
   - Проверьте logcat на наличие ошибок AdMob
   - Реклама должна показываться с меткой "Test Ad" (если используете тестовые ID)

### IAP:
1. **В редакторе**: Unity Fake Store (тестовые покупки)
2. **На устройстве (Debug)**: Google Play тестовые покупки
3. **На устройстве (Release)**: Реальные покупки

---

## 🔧 Проверка перед сборкой

### Checklist:
- ✅ App ID настроен через Assets → Google Mobile Ads → Settings
- ✅ AndroidManifest.xml содержит App ID
- ✅ Все рекламные блоки созданы в AdMob Console
- ✅ Все продукты созданы в Google Play Console
- ✅ Продукты активированы (не в черновике)

---

## 🚀 Сборка и публикация

### Для тестирования:
1. Build Settings → Android
2. Build Type: Development Build
3. Установите на устройство
4. Проверьте работу рекламы и покупок

### Для релиза:
1. Build Settings → Android
2. Build Type: Release
3. Подпишите APK/AAB
4. Загрузите в Google Play Console

---

## ❗ Частые проблемы

### Реклама не показывается:
- Подождите 5-10 минут после первого запуска
- Проверьте logcat: `adb logcat | grep -i admob`
- Убедитесь, что App ID правильно настроен
- Проверьте интернет-соединение

### IAP не работает:
- Убедитесь, что продукты активированы в Google Play Console
- Проверьте, что приложение подписано тем же ключом
- Для тестирования добавьте тестовый аккаунт в Google Play Console

---

## 📚 Источники

- [Google AdMob Unity Quick Start](https://developers.google.com/admob/unity/quick-start)
- [Unity IAP Documentation](https://docs.unity3d.com/Packages/com.unity.purchasing@latest)
