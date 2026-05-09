using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;
    [HideInInspector] public bool isFirst = false;
    private float speed = 1f;
    private bool isReset = false;
    public int damage = 1;
    public SpriteRenderer spriteBall;
    
    private bool _isDestoryOn = false;
    private static string lastConfiguredScene = "";

    private void Awake()
    {
        // Настройка игнорирования коллизий между шариками при смене сцены
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Конфигурируем коллизии только если сцена изменилась
        if (lastConfiguredScene != currentScene)
        {
            int ballLayer = gameObject.layer;

            // Игнорируем столкновения между шариками ТОЛЬКО на сцене 3_Play
            // На сцене 5_Arcada шарики будут сталкиваться друг с другом
            if (currentScene == Data.scene_play)
            {
                Physics2D.IgnoreLayerCollision(ballLayer, ballLayer, true);
            }
            else
            {
                Physics2D.IgnoreLayerCollision(ballLayer, ballLayer, false);
            }

            lastConfiguredScene = currentScene;
        }
    }

    public void SetData(int damage)
    {
        this.damage = damage;
        isReset = false;
        _isDestoryOn = false;
        rb.AddRelativeForce(Player.instance.shotRot.transform.up.normalized * speed, ForceMode2D.Impulse);
        GetComponent<CircleCollider2D>().enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, не шарик ли это (дополнительная защита)
        if (collision.gameObject.GetComponent<Ball>() != null)
            return;
            
        CtrGame.instance.ShotSound();
        _isDestoryOn = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDestoryOn)
        {
            if (collision.CompareTag("InTrigger"))
            {
                if (!Player.instance.isFirst)
                {
                    Player.instance.isFirst = true;
                    Player.instance.SetNextPositionX(transform.position.x);
                    Reset();
                }
                else
                {
                    MoveBall();
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isDestoryOn)
        {
            if (collision.CompareTag("InTrigger"))
            {
                if (!Player.instance.isFirst)
                {
                    Player.instance.isFirst = true;
                    Player.instance.SetNextPositionX(transform.position.x);
                    Reset();
                }
                else
                {
                    MoveBall();
                }
            }
        }
    }

    public void MoveBall()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        rb.linearVelocity = Vector3.zero;
        transform.DOKill();
        transform.DOMove(Player.instance.nextPosition, 0.15f).SetEase(Ease.OutCubic).OnComplete(() => { Reset(); });
    }

    public void ReturnBall()
    {
        rb.linearVelocity = Vector3.zero;
        GetComponent<CircleCollider2D>().enabled = false;
        transform.DOMove(Player.instance.nextPosition, 0.25f).SetEase(Ease.OutCubic).OnComplete(() => { Reset(); });
    }

    private void Reset()
    {
        if (!isReset)
        {
            isReset = true;
            isFirst = false;
            _isDestoryOn = false;
            Player.instance.activeBall.Remove(this);
            PoolManager.Despawn(this.gameObject);
        }
    }
}