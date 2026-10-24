using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.CoreLibs;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Helper
{

    public static class ApplicationMemoryHelper
    {
        public static CRUDResultModel GetTopMemoryObjects([FromQuery] int top = 30)
        {
            return new CRUDResultModel();
        }

        //public static CRUDResultModel GetTopMemoryObjects([FromQuery] int top = 30)
        //{
        //    // 1. 取得目前 API 站台自身的 Process ID
        //    int pid = Process.GetCurrentProcess().Id;

        //    try
        //    {
        //        // 2. 建立當前進程的快照並附隨（Attach）上去
        //        using var dataTarget = DataTarget.AttachToProcess(pid , suspend: false);

        //        // 取得當前的 CLR 運行時資訊
        //        var version = dataTarget.ClrVersions.FirstOrDefault();

        //        if (version == null)
        //            return new CRUDResultModel("無法取得當前 .NET Core CLR 版本資訊。");                      
                     
        //        using var runtime = version.CreateRuntime();
        //        var heap = runtime.Heap;

        //        // 檢查當前 Heap 是否處於可讀取狀態
        //        if (!heap.CanWalkHeap)
        //            return new CRUDResultModel("當前垃圾回收正在執行中或 Heap 狀態毀損，無法掃描。請稍後再試。");

        //        // 3. 核心大掃除：逐一掃描 Heap 上的所有物件並進行 Group By 統計

        //        // 1. 建立一個 Dictionary 用來在記憶體中累加統計，只佔用極小的空間
        //        var statsMap = new Dictionary<string, (int Count, long TotalSize)>();

        //        foreach (ClrObject obj in heap.EnumerateObjects())
        //        {
        //            // 防線一：過濾無效物件與殘影
        //            if (!obj.IsValid) 
        //                continue;

        //            try
        //            {
        //                // 防線二：安全地獲取 Type，如果 Type 剛好被回收就忽略這一個
        //                var type = obj.Type;

        //                if (type == null) 
        //                    continue;

        //                string typeName = type.Name ?? "Unknown/Free Space";
        //                long size = (long)obj.Size; // 取得物件大小

        //                // 進行累加
        //                if (statsMap.TryGetValue(typeName, out var current))
        //                {
        //                    statsMap[typeName] = (current.Count + 1, current.TotalSize + size);
        //                }
        //                else
        //                {
        //                    statsMap[typeName] = (1, size);
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                // 防線三：如果在讀取 Type.Name 或 Size 的微秒間物件被 GC 移動，直接忽略跳過
        //                continue;
        //            }
        //        }

        //        // 2. 迴圈跑完後，這時大局已定，再對這個「小小的 Dictionary」使用 LINQ 進行排序與 Take
        //        var objectStats = statsMap
        //            .Select(kvp => new
        //            {
        //                ClassName = kvp.Key,
        //                Count = kvp.Value.Count,
        //                TotalSizeInBytes = kvp.Value.TotalSize,
        //                TotalSizeInMB = Math.Round((double)kvp.Value.TotalSize / 1024 / 1024, 2),
        //                AverageSizeInBytes = kvp.Value.TotalSize / kvp.Value.Count
        //            })
        //            .OrderByDescending(x => x.TotalSizeInBytes)
        //            .Take(top)
        //            .ToList();

        //        return new CRUDResultModel
        //        {
        //            Message = "成功抓取前幾名最佔記憶體的物件類別",
        //            ResultExtraString = RuntimeHelper.GetK8sInstanceName(),
        //            ResultObject = objectStats
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new CRUDResultModel("記憶體掃描失敗" + ex.Message + "、" + ex.InnerException?.Message);
        //    }
        //}


    }
}
