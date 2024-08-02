using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 스테이지 관련
// 1. 스테이지 정보 (보드 모양, 목표, 이동 수 제한 등)
// 2. 스테이지 클리어 판단
public class StageManager : MonoBehaviour
{
    [Serializable]
    public struct StageQuestUnit
    {
        public CellShapeType questCellShapeType;  // 깨야하는 cell 종류
        public int questCellNum;        // 깨야하는 cell 개수
    }
    
    [SerializeField] private int _stageNum = 0;
    [SerializeField] private int _boardSize = 9;
    [SerializeField] private StageQuestUnit[] _stageGoal;

    private BoardController _boardController;

    private void Start()
    {
        //InitStage(stageNum, boardSize);
    }

    // // stage 구성요소 생성 및 초기화
    // private void InitStage(int stageNum, int boardSize)
    // {
    //     
    // }
}
