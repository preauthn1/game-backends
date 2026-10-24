<?php

namespace App\Http\Controllers\Member;

use Carbon\Carbon;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use Illuminate\Support\Facades\Validator;
use App\Http\Controllers\Controller;
use App\Enums\MinerEnum as MinerEnum;
use App\Helpers\MinerHelper as MinerHelper;
use App\Repositories\MemberMinerRepository;
use App\Services\MemberMinerService;
use Exception;

class MinerController extends Controller
{
    /**
     * 獲取我的礦機列表(新到舊)
     *
     * @bodyParam miner_type int required 類型 0=rent; 1=purchase
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "mm_id": 52, 玩家礦機id
     *         "name": "L7", 礦機型號
     *         "image": "https://XXXXXX/miner/L7.png", 礦機圖片
     *         "type": 1, 類型 0=租; 1=買; 2=團購
     *         "created_at": "2024-08-14 12:27:36", 創建日期
     *         "end_time": "2027-08-14 15:59:59", 結束日期
     *       },
     *     ]
     *   }
     * }
     */
    public function getMyMiners(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_type' => 'required|digits_between:1,2',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            //錯誤的類型
            if (!in_array($request['miner_type'], MinerEnum::MinerType)) {
                throw new Exception(__('error_message.5024'), 5024);
            }
            //類型 1=purchase包含購買與團購
            if (1 == $request['miner_type']) {
                $typeList = [MinerEnum::MinerType['purchase'], MinerEnum::MinerType['group']];
            } else {
                $typeList = [$request['miner_type']];
            }
            $memberMinerRepository = new MemberMinerRepository();
            $myMinerList = $memberMinerRepository->getMyMiners([
                'typeList' => $typeList,
                'member_id' => $request->ID,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $myMinerList
                ]
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取我的礦機詳情
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "mm_id": 56, 玩家礦機id
     *         "miner_id": 1, 礦機ID
     *         "name": "S19K", 礦機型號
     *         "image": "https://XXXXXX/miner/S19K.png", 礦機圖片
     *         "type": 1, 類型 0=租; 1=買; 2=團購
     *         "sell_back": {
     *             "is_show": 1, 資格 0=無資格; 1=有資格;
     *             "is_active": 0, 狀態 0=不可售回; 1=可售回;
     *         },
     *         "status": {
     *             "total_revenue": 0, 累計總收益
     *             "next_revenue_time": "08/26 14:00", 下一次收益計算時間
     *             "day_elec_fee": "8.17", 每日電費
     *             "custody_fee": "1.5", 託管費
     *             "custody_fee_rate": "0.8", 託管費比例
     *             "next_depreciable_time": "09/08", 下一次折舊計算日
     *             "available_cycles": 4, 租賃有效週期
     *             "end_time": "08/20 23:59" 礦機結束時間
     *         },
     *         "detail": {
     *             "create_time": "2024/08/20 11:13", 購買時間
     *             "start_time": "2024/08/21 00:00", 礦機開始時間
     *             "mining_currency": "BTC", 挖礦幣種
     *             "hash_rate": "120", 算力
     *             "power_consumption": "3250", 功耗
     *             "day_revenue": "3.0700", 每日收益
     *             "depreciation_cycle": 2, 折舊周期
     *             "balance": 0, 礦機殘值
     *         },
     *         "revenue_list": [
     *             {
     *                 "date": "Apr 21 2024", 收益日期
     *                 "currency": "BTC", 收益幣種
     *                 "revenue": "0.000003548" 收益金額
     *             }
     *         ]
     *       }
     *     ]
     *   }
     * }
     */
    public function getMyMinerDetail(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberMinerRepository = new MemberMinerRepository();
            $myMinerDetail = $memberMinerRepository->getMyMinerDetail([
                'mm_id' => $request->mm_id,
                'member_id' => $request->ID,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            //無對應id紀錄
            if (empty($myMinerDetail)) {
                throw new Exception(__('error_message.5028'), 5028);
            }
            //返回數據整理
            $minerHelper = new MinerHelper();
            $sellBack = $minerHelper->sellBack($myMinerDetail);
            $residual_value = $minerHelper->calculateRemain([
                'mm_id' => $request->mm_id,
                'member_id' => $request->ID
            ]);
            $infos = json_decode($myMinerDetail['info'], true);

            $memberMinerService = new MemberMinerService();
            $list = [
                'mm_id' => $myMinerDetail['mm_id'],
                'miner_id' => $myMinerDetail['id'],
                'name' => $myMinerDetail['name'],
                'image' => $myMinerDetail['image'],
                'type' => $myMinerDetail['type'],
                'sell_back' => [
                    'is_show' => $sellBack['qualify'],//是否展示
                    'is_active' => $sellBack['status'],//是否高亮
                ],
                'status' => [
                    'total_revenue' => 0,//miner todo 累計總收益
                    'next_revenue_time' => Carbon::parse($minerHelper->nextRevenueTime())->format('m/d H:i'),
                    'day_elec_fee' => $infos['day_elec_fee'] ?? 0,
                    'custody_fee' => $infos['custody_fee'] ?? 0,
                    'custody_fee_rate' => $infos['custody_fee_rate'] ?? 0,
                    'next_depreciable_time' => Carbon::parse($minerHelper->nextDepreciableTime($request->mm_id, ($myMinerDetail['depreciation_cycle'] ?: MinerEnum::DefaultDepreciationDays)))->format('m/d'),
                    'available_cycles' => 4,//miner todo 剩餘週期
                    'end_time' => Carbon::parse($myMinerDetail['end_time'])->format('m/d H:i'),
                ],
                'details' => [
                    'create_time' => Carbon::parse($myMinerDetail['created_at'])->format('Y/m/d H:i'),
                    'start_time' => Carbon::parse($myMinerDetail['start_time'])->format('Y/m/d H:i'),
                    'mining_currency' => $myMinerDetail['mining_currency'],
                    'hash_rate' => $infos['hash_rate'],
                    'power_consumption' => $infos['power_consumption'],
                    'energy_efficiency' => round(($infos['power_consumption'] / $infos['hash_rate']), 2),
                    'day_revenue' => $myMinerDetail['day_revenue'],
                    'depreciation_cycle' => $myMinerDetail['depreciation_cycle'] / MinerEnum::CycleDays,
                    'balance' => $residual_value,
                ],
                //最近14筆礦機收益
                'revenue_list' => $memberMinerService->getRevenueList($request->mm_id)
            ];
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
                ]
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }
}
