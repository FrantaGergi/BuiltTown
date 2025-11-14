using UnityEngine;

public class Mailbox : MonoBehaviour
{
    [SerializeField, Header("References")]
    private UIMailbox uiMailbox;
    [SerializeField]
    private MailboxArrow mailboxArrow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     mailboxArrow.ShowArrow();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
