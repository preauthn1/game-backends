using System;
using System.Collections.Generic;
using System.Linq;

public class SuperToolAssignRewardDetail
{

    public int Idx { get; set; }
    /// <summary>
    /// 指定開獎
    /// </summary>
    public string Description { get; set; }
    public int Status { get; set; }
    /// <summary>
    /// 實際開獎結果
    /// </summary>
    public string Reward { get; set; }
    /// <summary>
    /// 主表狀態 0 取消  1 啟用 2 暫停
    /// </summary>
    public int MainStatus { get; set; }

    private string _strStatus;
    public string StrStatus
    {
        get { return _strStatus; }
        set
        {
            switch (Status)
            {
                case 1:
                    _strStatus = "进行中";
                    break;
                case 2:
                    _strStatus = "己完成";
                    break;
                case 3:
                    _strStatus = "指定开奖";
                    break;
                default:
                    _strStatus = "未完成";
                    break;
            }
        }
    }

}