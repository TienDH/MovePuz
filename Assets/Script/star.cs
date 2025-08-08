using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private GameObject breakEffect; // Hiệu ứng phá vỡ
    private bool isCollected = false;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (breakEffect != null)
        {
            breakEffect.SetActive(false);
            breakEffect.transform.SetParent(null); // Đảm bảo không là con của Star
            ParticleSystem ps = breakEffect.GetComponent<ParticleSystem>();
            if (ps != null && ps.isPlaying)
            {
                ps.Stop(); // Dừng Particle System nếu đang chạy
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected || !gameObject.activeInHierarchy) return; // Ngăn gọi khi đã thu thập hoặc không active

        if (collision.CompareTag("Player"))
        {
            isCollected = true;
            if (gameManager != null)
            {
                gameManager.WinGame();
            }
            else
            {
                Debug.LogError("GameManager not found in Star");
            }

            if (breakEffect != null)
            {
                // Đặt vị trí hiệu ứng tại vị trí ngôi sao
                breakEffect.transform.position = transform.position;
                breakEffect.SetActive(true);
                // Phát Particle System thủ công
                ParticleSystem ps = breakEffect.GetComponent<ParticleSystem>();
                if (ps != null && !ps.isPlaying)
                {
                    ps.Play(); // Phát particle nếu chưa chạy
                }
                // Tắt hiệu ứng sau khi hoàn tất
                float duration = ps != null ? ps.main.duration : 2f;
                StartCoroutine(DeactivateEffectAndStar(duration));
            }
            else
            {
                // Nếu không có hiệu ứng, vô hiệu hóa ngay
                gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator DeactivateEffectAndStar(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (breakEffect != null)
        {
            breakEffect.SetActive(false);
            ParticleSystem ps = breakEffect.GetComponent<ParticleSystem>();
            if (ps != null && ps.isPlaying)
            {
                ps.Stop(); // Dừng Particle System sau khi hoàn tất
            }
        }
        gameObject.SetActive(false); // Vô hiệu hóa Star sau khi hiệu ứng hoàn tất
    }

    // Reset trạng thái khi tái sử dụng ngôi sao
    public void ResetStar()
    {
        isCollected = false;
        gameObject.SetActive(true);
        if (breakEffect != null)
        {
            breakEffect.SetActive(false);
            ParticleSystem ps = breakEffect.GetComponent<ParticleSystem>();
            if (ps != null && ps.isPlaying)
            {
                ps.Stop(); // Dừng Particle System nếu đang chạy
            }
        }
    }

    // Phương thức để gán breakEffect từ MatrixGame
    public void SetBreakEffect(GameObject effect)
    {
        // Hủy hiệu ứng cũ nếu tồn tại
        if (breakEffect != null && breakEffect != effect)
        {
            Destroy(breakEffect); // Hủy instance cũ
        }
        breakEffect = effect;
        if (breakEffect != null)
        {
            breakEffect.SetActive(false); // Đảm bảo tắt ngay khi gán
            breakEffect.transform.SetParent(null); // Đảm bảo không là con của Star
            ParticleSystem ps = breakEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(); // Dừng Particle System nếu đang chạy
                ps.Clear(); // Xóa các particle còn lại
            }
        }
    }
}