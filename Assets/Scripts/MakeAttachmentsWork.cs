using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeAttachmentsWork : MonoBehaviour
{
    public GameObject[] attachments;
	void OnEnable ()
    {
		foreach (GameObject attachment in attachments)
        {
            attachment.SetActive(true);
        }
	}
}