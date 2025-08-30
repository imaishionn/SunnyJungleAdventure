// JumpButtonController.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class JumpButtonController : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.GetPlayerMove() != null)
        {
            GameManager.Instance.GetPlayerMove().Jump();
        }
    }
}