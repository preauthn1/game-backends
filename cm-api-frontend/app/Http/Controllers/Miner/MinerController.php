<?php

namespace App\Http\Controllers\Miner;

use App\Models\TransactionLog;
use Carbon\Carbon;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Collection;
use App\Enums\TimeEnum as TimeEnum;
use App\Enums\MinerEnum as MinerEnum;
use App\Enums\WalletTypeEnum;
use App\Enums\TransactionTypeEnum as TransactionTypeEnum;
use App\Enums\AgentCustomizedSettingsEnum;
use App\Helpers\MinerHelper;
use App\Helpers\StringHelper;
use App\Helpers\Common;
use App\Http\Controllers\Controller;
use App\Models\Miner;
use App\Models\MemberMiner;
use App\Models\MemberMinerTransactionLog;
use App\Services\MemberService;
use App\Services\MinerService;
use App\Services\MemberMinerService;
use App\Services\TransactionLogService;
use App\Services\AgentService;
use App\Repositories\MinerRepository;
use App\Repositories\MemberMinerRepository;
use Exception;

class MinerController extends Controller
{
    /**
     * 獲取待出售礦機列表(按sort高到低)
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "count": 3, 可售數量
     *         "id": 4, 礦機ID
     *         "name": "L7", 礦機型號
     *         "image": "https://XXXXXX/miner/L7.png", 礦機圖片
     *         "purchase_price": "2416.0000", 出售價格
     *         "day_revenue": "3.8600", 每日收益
     *         "depreciation_cycle": 2, 折舊周期
     *         "purchase_total": 3, 總可售數量
     *         "status": 1, 狀態 0=禁用; 1=啟用
     *         "p_num": 1, 買一台礦機可租數量
     *         "p_cycle": 2, 買一台礦機可租週期
     *         "g_num": 1, 團購一台礦機可租數量
     *         "g_cycle": 2, 團購一台礦機可租週期
     *         "mining_currency": "BTC", 挖礦幣種
     *         "hash_rate": "160", 算力
     *         "power_consumption": "3375" 功耗
     *         "tag": "hot", 標籤文字
     *         "tag_color": "#FF9900" 標籤底色
     *       },
     *     ]
     *   }
     * }
     */
    public function getPurchase(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            $minerRepository = new MinerRepository();
            $purchaseList = $minerRepository->getPurchaseMiners([
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP
            ]);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $this->doMultiMinerData($purchaseList, ['rent_price'])
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
     * 購買礦機
     *
     * @bodyParam miner_id int required 礦機ID
     * @bodyParam quantity int required 購買數量
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function doPurchase(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|digits_between:1,10',
                'quantity' => 'required|digits_between:1,4',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            DB::beginTransaction();
            //購買礦機是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::PURCHASE_MINER_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?: 0;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->ID);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            //確認剩餘數量
            $minerRepository = new MinerRepository();
            $minerList = $minerRepository->getPurchaseMiners([
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
                'miner_id' => $request->miner_id
            ]);
            if (empty($minerList)) {
                throw new Exception(__('error_message.5023'), 5023);
            }
            //取第一筆
            $miner = $minerList[0];
            $pendingCount = $miner->count ?? 0;
            $purchasePrice = $miner->purchase_price ?? 0;
            $totalPurchasePrice = ($request->quantity * $purchasePrice);

            //取得錢包餘額
            $memberService = new MemberService();
            $wallet = $memberService->getMainWallet($request->ID);
            if ($wallet->balance < $totalPurchasePrice) {
                throw new Exception(__('error_message.1500'), 1500);
            }

            //數量不足
            if ($pendingCount < $request->quantity) {
                return response()->json([
                    'code' => 2003,
                    'message' => __('error_message.2003'),
                    'data' => [
                        'count' => $pendingCount
                    ]
                ]);
            }
            $minerTimes = $this->getMinerTimesUtc();

            list($before_balance, $after_balance) = $memberService->walletDecrement(
                $request->ID,
                $totalPurchasePrice,
                WalletTypeEnum::MAIN
            );

            //創建購買
            $memberMinerPurchase = [];
            for ($i = 0; $i < $request->quantity; $i++) {
                $memberMinerPurchase[] = [
                    'type' => MinerEnum::MinerType['purchase'],
                    'member_id' => $request->ID,
                    'miner_id' => $request->miner_id,
                    'check_time' => Carbon::now(),
                    'start_time' => $minerTimes['start'],
                    'end_time' => $minerTimes['end'],
                    'can_sellback_time' => $minerTimes['can_sellback'],
                    'status' => 1,
                ];
            }
            $resultPurchase = MemberMiner::insert($memberMinerPurchase);

            //抓取id
            $memberMinerRepository = new MemberMinerRepository();
            $resultList = $memberMinerRepository->getLatestData([
                'type' => MinerEnum::MinerType['purchase'],
                'member_id' => $request->ID,
                'miner_id' => $request->miner_id,
                'limit' => $request->quantity
            ]);
            $purchaseIds = collect($resultList)->sortBy('id')->pluck('id')->toArray();
            //購買礦機可租數量
            $promos = $this->getPromo($miner->promo);
            $p_num = $promos['p_num'] ?? 0;
            //創建租賃
            $memberMinerRent = [];
            foreach ($purchaseIds as $id) {
                for ($i = 0; $i < $p_num; $i++) {
                    $memberMinerRent[] = [
                        'type' => MinerEnum::MinerType['rent'],
                        'member_id' => $request->ID,
                        'miner_id' => $request->miner_id,
                        'parent_id' => $id,
                        'status' => 0,
                        'info' => '{"cycle_days":' . ($promos['p_days'] ?? 0) . '}'
                    ];
                }
            }
            $resultRent = MemberMiner::insert($memberMinerRent);
            if (!$resultPurchase || !$resultRent) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_PURCHASE,
                'order_id' => 0,
                'point' => $totalPurchasePrice,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            foreach ($purchaseIds as $id) {
                $memberMinerTransactionLog[] = [
                    'transaction_log_id' => $resultTransactionLogId,
                    'member_miner_id' => $id,
                ];
            }
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取待出租礦機列表(按sort高到低)
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "count": 4, 可租數量
     *         "id": 1, 礦機ID
     *         "name": "S19k", 礦機型號
     *         "image": "https://XXXXXX/miner/S19K.png", 礦機圖片
     *         "rent_price": "18.0000", 出租價格
     *         "day_revenue": "3.8600", 每日收益
     *         "status": 1, 狀態 0=禁用; 1=啟用
     *         "mining_currency": "BTC", 挖礦幣種
     *         "hash_rate": "120", 算力
     *         "power_consumption": "3250" 功耗
     *         "tag": "hot", 標籤文字
     *         "tag_color": "#FF9900" 標籤底色
     *       },
     *     ]
     *   }
     * }
     */
    public function getRentAll(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            $minerRepository = new MinerRepository();
            $rentAll = $minerRepository->getRentMiners([
                'member_id' => $request->ID,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $this->doMultiMinerData($rentAll, ['purchase_price', 'depreciation_cycle', 'purchase_total', 'promo'])
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
     * 獲取特定型號待出租礦機列表(按週期多到少)
     *
     * @bodyParam miner_id int required 礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "mm_id": 72, 用戶礦機ID
     *         "id": 3, 礦機ID
     *         "name": "MarsXXX", 礦機型號
     *         "image": "https://XXXXXX/miner/MarsXXX.png", 礦機圖片
     *         "rent_price": "11.0000", 出租價格
     *         "day_revenue": "1.0000", 每日收益
     *         "status": 1, 狀態 0=禁用; 1=啟用
     *         "cycle": 2, 週期
     *         "mining_currency": "BTC", 挖礦幣種
     *         "hash_rate": "160", 算力
     *         "power_consumption": "3375" 功耗
     *         "tag": "special", 標籤文字
     *         "tag_color": "#ca473f" 標籤底色
     *       },
     *     ]
     *   }
     * }
     */
    public function getRentList(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $minerRepository = new MinerRepository();
            $rentList = $minerRepository->getRentMinersList([
                'member_id' => $request->ID,
                'miner_id' => $request->miner_id,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            if (empty($rentList)) {
                throw new Exception(__('error_message.5023'), 5023);
            }
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $this->doMultiMinerData($rentList, ['purchase_price', 'depreciation_cycle', 'purchase_total', 'promo'])
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
     * 租礦機
     *
     * @bodyParam mm_ids array required 會員礦機IDs
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function doRent(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_ids' => 'required|array|min:1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            DB::beginTransaction();
            //租礦機是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::RENT_MINER_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?: 0;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->ID);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            //取資訊
            $memberMiners = MemberMiner::whereIn('id', $request->mm_ids)->get()->toArray();
            //全部會員礦機ID列表錯誤
            if (empty($memberMiners)) {
                throw new Exception(__('error_message.5027') . '(1)', 5027);
            }
            //部分會員礦機ID列表錯誤
            if (count($memberMiners) != count($request->mm_ids)) {
                throw new Exception(__('error_message.5027') . '(2)', 5027);
            }

            $memberMiner = $memberMiners[0];
            $minerService = new MinerService();
            $miner = $minerService->getById($memberMiner['miner_id']);
            $totalRentalPrice = ($miner->rent_price * count($request->mm_ids));

            //體驗的礦機數量
            $experienceCount = 0;
            $totalExperienceCommission = 0;
            foreach ($memberMiners as $mmData) {
                if ($mmData['operator'] > 0) {
                    $experienceCount++;
                }
            }
            if ($experienceCount > 0) {
                $promos = $this->getPromo($miner->promo);
                $totalExperienceCommission = bcmul(floatval($promos['r_ec']), $miner->rent_price, 6) * $experienceCount;
            }

            //取得錢包餘額
            $memberService = new MemberService();
            $wallet = $memberService->getMainWallet($request->ID);
            if ($wallet->balance < $totalRentalPrice) {
                throw new Exception(__('error_message.1500'), 1500);
            }

            list($before_balance, $after_balance) = $memberService->walletDecrement(
                $request->ID,
                $totalRentalPrice,
                WalletTypeEnum::MAIN
            );

            //租礦機
            $rentUpdateData = [];
            foreach ($memberMiners as $data) {
                //礦機類型非租賃
                if (MinerEnum::MinerType['rent'] != $data['type']) {
                    throw new Exception(__('error_message.5027') . '(3)', 5027);
                }
                //礦機已租
                if (!empty($data['end_time'])) {
                    throw new Exception(__('error_message.5027') . '(4)', 5027);
                }
                $infos = json_decode($data['info'], true);
                $p_days = intval($infos['cycle_days'] ?? 0);
                if (empty($p_days)) {
                    throw new Exception(__('error_message.5026'), 5026);
                }
                $minerTimes = $this->getMinerTimesUtc($p_days);
                $rentUpdateData[] = [
                    'id' => $data['id'],
                    'check_time' => Carbon::now(),
                    'start_time' => $minerTimes['start'],
                    'end_time' => $minerTimes['end'],
                    'can_sellback_time' => $minerTimes['can_sellback'],
                    'status' => 1,
                ];
            }
            $resultRent = MemberMiner::upsert($rentUpdateData, ['id'], ['check_time', 'start_time', 'end_time', 'can_sellback_time', 'status']);
            if (!$resultRent) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_RENTAL,
                'order_id' => 0,
                'point' => $totalRentalPrice,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            foreach ($memberMiners as $data) {
                $memberMinerTransactionLog[] = [
                    'transaction_log_id' => $resultTransactionLogId,
                    'member_miner_id' => $data['id'],
                ];
            }
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //體驗返佣
            if ($totalExperienceCommission > 0) {
                list($before_balance, $after_balance) = $memberService->walletIncrement(
                    $request->ID,
                    $totalExperienceCommission,
                    WalletTypeEnum::MAIN
                );
                $transactionLog = [
                    'member_id' => $request->ID,
                    'wallet_type_id' => WalletTypeEnum::MAIN,
                    'transaction_type_id' => TransactionTypeEnum::TYPE_EXP_COMMISSION,
                    'order_id' => $resultTransactionLogId,
                    'point' => $totalExperienceCommission,
                    'before_balance' => $before_balance,
                    'after_balance' => $after_balance,
                    'status' => 1,
                ];
                $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
                if (!$resultTransactionLogId) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取待團購礦機型號列表(按數量高到低)
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "count": 10, 可團購數量
     *         "id": 1, 礦機ID
     *         "name": "S19K", 礦機型號
     *         "image": "https://XXXXXX/miner/S19K.png", 礦機圖片
     *         "purchase_price": "968.5000", 出售價格
     *         "day_revenue": "1.5350", 每日收益
     *         "depreciation_cycle": 2, 折舊周期
     *         "status": 1, 狀態 0=禁用; 1=啟用
     *         "p_num": 1, 買一台礦機可租數量
     *         "p_cycle": 2, 買一台礦機可租週期
     *         "g_num": 1, 團購一台礦機可租數量
     *         "g_cycle": 2, 團購一台礦機可租週期
     *         "mining_currency": "BTC", 挖礦幣種
     *         "hash_rate": "120", 算力
     *         "power_consumption": "3250" 功耗
     *         "tag": "hot", 標籤文字
     *         "tag_color": "#FF9900" 標籤底色
     *       },
     *     ]
     *   }
     * }
     */
    public function getGroup(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            $minerRepository = new MinerRepository();
            $groupList = $minerRepository->getGroupMiners([
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $this->doMultiMinerData($groupList, ['rent_price', 'purchase_total'])
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
     * 創建團購
     *
     * @bodyParam miner_id int required 礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function createGroup(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            DB::beginTransaction();
            //團購礦機是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::GROUP_MINER_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?: 0;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->ID);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            //礦機id不存在
            $minerRepository = new MinerRepository();
            $miners = $minerRepository->findById([
                'id' => $request->miner_id,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            if (empty($miners)) {
                throw new Exception(__('error_message.5023'), 5023);
            }

            //取得錢包餘額
            $memberService = new MemberService();
            $wallet = $memberService->getMainWallet($request->ID);
            if ($wallet->balance < $miners->group_price) {
                throw new Exception(__('error_message.1500'), 1500);
            }

            list($before_balance, $after_balance) = $memberService->walletDecrement(
                $request->ID,
                $miners->group_price,
                WalletTypeEnum::MAIN
            );

            //創建團購
            $groupMiner = [
                'type' => MinerEnum::MinerType['group'],
                'member_id' => $request->ID,
                'miner_id' => $request->miner_id,
                'status' => 0,
            ];
            $resultGroup = MemberMiner::insertGetId($groupMiner);
            if (!$resultGroup) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_PURCHASE,
                'order_id' => 0,
                'point' => $miners->group_price,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            $memberMinerTransactionLog[] = [
                'transaction_log_id' => $resultTransactionLogId,
                'member_miner_id' => $resultGroup,
            ];
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取特定型號待團購礦機列表(自己在前其餘按創建舊到新)
     *
     * @bodyParam miner_id int required 礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "mm_id": 91, 團購礦機ID
     *         "username": "leahMiner02/te****02", 等待拚團用戶名(自己不隱碼)
     *         "type": 0, 類型 0=取消; 1=團購;
     *       },
     *     ]
     *   }
     * }
     */
    public function getGroupList(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $minerRepository = new MinerRepository();
            $groupList = $minerRepository->getGroupMinersList([
                'miner_id' => $request->miner_id,
                'member_id' => $request->ID,
                'organization_id' => $request->ORGANIZATION_ID,
                'agent_group_id' => $request->MINER_GROUP,
            ]);
            $groupList = collect($groupList)->map(function ($item) use ($request) {
                $item->type = 0;
                if ($item->member_id != $request->ID) {
                    $item->type = 1;
                    $item->username = StringHelper::stringEncrypt($item->username, 2, 2, 4);
                }
                unset($item->member_id);
                return $item;
            })->toArray();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $groupList
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
     * 團購礦機
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function doGroup(Request $request): \Illuminate\Http\JsonResponse
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

            //取資訊
            $memberMiners = MemberMiner::find($request->mm_id);
            //團購礦機是否實名判斷走配置
            $agentService = new AgentService();
            $request->merge([
                'setting_key' => AgentCustomizedSettingsEnum::GROUP_MINER_REQUIRES_VERIFIED_ID,
            ]);
            $settingResult = $agentService->getCustomizedSetting($request)?->value ?: 0;
            if (1 == $settingResult) {
                $memberService = new MemberService();
                $id_information = $memberService->getIdInformation($request->ID);
                if (1 != ($id_information['status'] ?? 9)) {
                    throw new Exception(__('error_message.1401'), 1401);
                }
            }
            /**/
            //無對應id紀錄
            if (empty($memberMiners)) {
                throw new Exception(__('error_message.7001') . '(1)', 7001);
            }
            //礦機類型非團購
            if (MinerEnum::MinerType['group'] != $memberMiners->type) {
                throw new Exception(__('error_message.7001') . '(2)', 7001);
            }
            //礦機已團購
            if (!empty($memberMiners->end_time)) {
                throw new Exception(__('error_message.7001') . '(3)', 7001);
            }
            //自己與自己團購
            if ($request->ID == $memberMiners->member_id) {
                throw new Exception(__('error_message.7001') . '(4)', 7001);
            }
            //狀態禁用或前端不展示
            if(1 != $memberMiners->status || 1 != $memberMiners->is_show){
                throw new Exception(__('error_message.7001') . '(5)', 7001);
            }

            $minerService = new MinerService();
            $miner = $minerService->getById($memberMiners->miner_id);

            $memberService = new MemberService();
            $wallet = $memberService->getMainWallet($request->ID);
            if ($wallet->balance < $miner->group_price) {
                throw new Exception(__('error_message.1500'), 1500);
            }

            DB::beginTransaction();

            list($before_balance, $after_balance) = $memberService->walletDecrement(
                $request->ID,
                $miner->group_price,
                WalletTypeEnum::MAIN
            );

            $minerTimes = $this->getMinerTimesUtc();
            //團購礦機
            $groupUpdateData = [
                'id' => $memberMiners->id,
                'parent_id' => $memberMiners->id,
                'check_time' => Carbon::now(),
                'start_time' => $minerTimes['start'],
                'end_time' => $minerTimes['end'],
                'can_sellback_time' => $minerTimes['can_sellback'],
                'status' => 1
            ];
            $resultUpdateGroup = MemberMiner::upsert($groupUpdateData, ['id'], ['parent_id', 'check_time', 'start_time', 'end_time', 'can_sellback_time', 'status']);
            //團購礦機可租數量
            $promos = $this->getPromo($miner->promo);
            $g_num = $promos['g_num'] ?? 0;
            //創建創團租賃
            $memberMinerRent = [];
            for ($i = 0; $i < $g_num; $i++) {
                $memberMinerRent[] = [
                    'type' => MinerEnum::MinerType['rent'],
                    'member_id' => $memberMiners->member_id,
                    'miner_id' => $memberMiners->miner_id,
                    'parent_id' => $memberMiners->id,
                    'status' => 0,
                    'info' => '{"cycle_days":' . ($promos['g_days'] ?? 0) . '}'
                ];
            }
            //創建併團
            $groupInsertData = [
                'type' => MinerEnum::MinerType['group'],
                'member_id' => $request->ID,
                'miner_id' => $memberMiners->miner_id,
                'parent_id' => $memberMiners->id,
                'check_time' => Carbon::now(),
                'start_time' => $minerTimes['start'],
                'end_time' => $minerTimes['end'],
                'can_sellback_time' => $minerTimes['can_sellback'],
                'status' => 1
            ];
            $resultInsertGroupId = MemberMiner::insertGetId($groupInsertData);
            //創建併團租賃
            for ($i = 0; $i < $g_num; $i++) {
                $memberMinerRent[] = [
                    'type' => MinerEnum::MinerType['rent'],
                    'member_id' => $request->ID,
                    'miner_id' => $memberMiners->miner_id,
                    'parent_id' => $resultInsertGroupId,
                    'status' => 0,
                    'info' => '{"cycle_days":' . ($promos['g_days'] ?? 0) . '}'
                ];
            }
            $resultRent = MemberMiner::insert($memberMinerRent);
            if (!$resultUpdateGroup || !$resultInsertGroupId || !$resultRent) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_PURCHASE,
                'order_id' => 0,
                'point' => $miner->group_price,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            $memberMinerTransactionLog[] = [
                'transaction_log_id' => $resultTransactionLogId,
                'member_miner_id' => $resultInsertGroupId,
            ];
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 取消團購礦機
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function cancelGroup(Request $request): \Illuminate\Http\JsonResponse
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
            DB::beginTransaction();
            //取資訊
            $memberMiners = MemberMiner::find($request->mm_id);
            //無對應mm_id
            if (empty($memberMiners)) {
                throw new Exception(__('error_message.7002') . '(1)', 7002);
            }
            //礦機類型非團購
            if (MinerEnum::MinerType['group'] != $memberMiners->type) {
                throw new Exception(__('error_message.7002') . '(2)', 7002);
            }
            //非自己的團購
            if ($request->ID != $memberMiners->member_id) {
                throw new Exception(__('error_message.7002') . '(3)', 7002);
            }
            //礦機團購已匹配
            if (!empty($memberMiners->cancel_time)) {
                throw new Exception(__('error_message.7002') . '(4)', 7002);
            }
            //礦機團購已匹配
            if (!empty($memberMiners->end_time)) {
                throw new Exception(__('error_message.7002') . '(5)', 7002);
            }
            //狀態禁用或前端不展示
            if(1 != $memberMiners->status || 1 != $memberMiners->is_show){
                throw new Exception(__('error_message.7002') . '(6)', 7002);
            }
            $now = Carbon::now()->toDateTimeString();
            //取消團購
            $groupUpdateData = [
                'id' => $request->mm_id,
                'end_time' => $now,
                'cancel_time' => $now,
                'status' => 2
            ];
            $resultUpdateGroup = MemberMiner::upsert($groupUpdateData, ['id'], ['end_time', 'cancel_time', 'status']);
            if (!$resultUpdateGroup) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //團購金額
            $minerService = new MinerService();
            $miner = $minerService->getById($memberMiners->miner_id);
            $groupPrice = $miner->group_price ?? 0;
            //額度加回
            $memberService = new MemberService();
            list($before_balance, $after_balance) = $memberService->walletIncrement(
                $request->ID,
                $groupPrice,
            );

            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_CANCEL_PURCHASE,
                'order_id' => 0,
                'point' => $groupPrice,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            $memberMinerTransactionLog[] = [
                'transaction_log_id' => $resultTransactionLogId,
                'member_miner_id' => $request->mm_id,
            ];
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 出售礦機
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function sellBackMiner(Request $request): \Illuminate\Http\JsonResponse
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
            //取資訊
            $memberMinerService = new MemberMinerService();
            $memberMiners = $memberMinerService->getById($request->mm_id);
            //無對應mm_id
            if (empty($memberMiners)) {
                throw new Exception(__('error_message.7003') . '(1)', 7003);
            }
            //非自己的礦機
            if ($request->ID != $memberMiners->member_id) {
                throw new Exception(__('error_message.7003') . '(2)', 7003);
            }
            //礦機類型為租賃無法售回
            if (MinerEnum::MinerType['rent'] == $memberMiners->type) {
                throw new Exception(__('error_message.7003') . '(3.1)', 7003);
            }
            //礦機類型為團購無法用此接口售回
            if (MinerEnum::MinerType['group'] == $memberMiners->type) {
                throw new Exception(__('error_message.7003') . '(3.2)', 7003);
            }
            //礦機未開始
            if (empty($memberMiners->end_time ?? '')) {
                throw new Exception(__('error_message.7003') . '(4)', 7003);
            }
            //礦機已過期
            if (Carbon::now()->gte($memberMiners->end_time)) {
                throw new Exception(__('error_message.7003') . '(5)', 7003);
            }
            //尚無法售回
            if (Carbon::now()->lte($memberMiners->can_sellback_time)) {
                throw new Exception(__('error_message.7003') . '(6)', 7003);
            }

            DB::beginTransaction();

            $now = Carbon::now();
            $infos = json_decode($memberMiners->info, true);
            //礦機殘值
            $memberMinerService = new MemberMinerService();
            $miner_residual_value = $memberMinerService->getResidualValue($request->mm_id);
            $infos['balance'] = $miner_residual_value;
            //出售購買礦機
            $sellBackUpdateData = [
                'id' => $request->mm_id,
                'end_time' => $now,
                'cancel_time' => $now,
                'status' => 2,
                'info' => json_encode($infos)
            ];
            $resultSellBack = MemberMiner::upsert($sellBackUpdateData, ['id'], ['end_time', 'cancel_time', 'status', 'info']);
            if (!$resultSellBack) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //額度加回
            $memberService = new MemberService();
            list($before_balance, $after_balance) = $memberService->walletIncrement(
                $request->ID,
                $miner_residual_value,
                WalletTypeEnum::MAIN
            );
            //TransactionLog
            $transactionLog = [
                'member_id' => $request->ID,
                'wallet_type_id' => WalletTypeEnum::MAIN,
                'transaction_type_id' => TransactionTypeEnum::TYPE_SELL_MINING,
                'order_id' => 0,
                'point' => $miner_residual_value,
                'before_balance' => $before_balance,
                'after_balance' => $after_balance,
                'status' => 1,
            ];
            $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
            if (!$resultTransactionLogId) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //MemberMinerTransactionLog
            $memberMinerTransactionLog = [];
            $memberMinerTransactionLog[] = [
                'transaction_log_id' => $resultTransactionLogId,
                'member_miner_id' => $request->mm_id,
            ];
            $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
            if (!$resultMemberMinerTransactionLog) {
                throw new Exception(__('error_message.2001'), 2001);
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 出售團購礦機
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     * }
     */
    public function sellBackGroupMiner(Request $request): \Illuminate\Http\JsonResponse
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
            //取資訊
            $memberMiners = MemberMiner::find($request->mm_id);
            if (empty($memberMiners->parent_id ?? '')) {
                $relationMemberMiner = MemberMiner::where('parent_id', $request->mm_id)->first();
            } else {
                $relationMemberMiner = MemberMiner::find($memberMiners->parent_id);
            }
            //無對應mm_id
            if (empty($memberMiners)) {
                throw new Exception(__('error_message.7003') . '(1)', 7003);
            }
            //非自己的礦機
            if ($request->ID != $memberMiners->member_id) {
                throw new Exception(__('error_message.7003') . '(2)', 7003);
            }
            //礦機類型為租賃無法售回
            if (MinerEnum::MinerType['rent'] == $memberMiners->type) {
                throw new Exception(__('error_message.7003') . '(3.1)', 7003);
            }
            //礦機類型為購買無法用此接口售回
            if (MinerEnum::MinerType['purchase'] == $memberMiners->type) {
                throw new Exception(__('error_message.7003') . '(3.2)', 7003);
            }
            //礦機未開始
            if (empty($memberMiners->end_time ?? '')) {
                throw new Exception(__('error_message.7003') . '(4)', 7003);
            }
            //礦機已過期
            if (Carbon::now()->gte($memberMiners->end_time)) {
                throw new Exception(__('error_message.7003') . '(5)', 7003);
            }
            //尚無法售回
            if (Carbon::now()->lte($memberMiners->can_sellback_time)) {
                throw new Exception(__('error_message.7003') . '(6)', 7003);
            }
            //狀態非進行中
            if (1 != $memberMiners->status) {
                throw new Exception(__('error_message.7003') . '(7)', 7003);
            }

            DB::beginTransaction();

            $now = Carbon::now();
            //出售團購礦機
            if (3 == $relationMemberMiner->status) {
                $memberMinerService = new MemberMinerService();
                //相關團購單都改狀態為2結束日期為當前 當前單加上取消時間為當前
                $memberMinersUpdateData = [
                    'id' => $request->mm_id,
                    'end_time' => $now,
                    'cancel_time' => $now,
                    'status' => 2,
                ];
                $resultSellBackMain = MemberMiner::upsert($memberMinersUpdateData, ['id'], ['end_time', 'cancel_time', 'status']);
                //用戶團購礦機殘值
                $resultSellBackMainResidualValue = $memberMinerService->getResidualValue($request->mm_id);

                $relationMemberMinersUpdateData = [
                    'id' => $relationMemberMiner->id,
                    'end_time' => $now,
                    'status' => 2,
                ];
                $resultSellBackRelation = MemberMiner::upsert($relationMemberMinersUpdateData, ['id'], ['end_time', 'status']);
                //相關單團購礦機殘值
                $resultSellBackRelationResidualValue = $memberMinerService->getResidualValue($relationMemberMiner->id);
                if (!$resultSellBackMain || !$resultSellBackRelation) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
                $memberService = new MemberService();
                $transactionLogService = new TransactionLogService();
                //用戶額度加回+額度紀錄
                list($before_balance, $after_balance) = $memberService->walletIncrement(
                    $request->ID,
                    $resultSellBackMainResidualValue,
                );
                //TransactionLog
                $transactionLog = [
                    'member_id' => $request->ID,
                    'wallet_type_id' => WalletTypeEnum::MAIN,
                    'transaction_type_id' => TransactionTypeEnum::TYPE_SELL_MINING,
                    'order_id' => 0,
                    'point' => $resultSellBackMainResidualValue,
                    'before_balance' => $before_balance,
                    'after_balance' => $after_balance,
                    'status' => 1,
                ];
                $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
                if (!$resultTransactionLogId) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
                //MemberMinerTransactionLog
                $memberMinerTransactionLog = [];
                $memberMinerTransactionLog[] = [
                    'transaction_log_id' => $resultTransactionLogId,
                    'member_miner_id' => $request->mm_id,
                ];
                $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
                if (!$resultMemberMinerTransactionLog) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
                //相關單額度加回+額度紀錄
                list($before_balance, $after_balance) = $memberService->walletIncrement(
                    $relationMemberMiner->member_id,
                    $resultSellBackRelationResidualValue,
                );
                //TransactionLog
                $transactionLog = [
                    'member_id' => $relationMemberMiner->member_id,
                    'wallet_type_id' => WalletTypeEnum::MAIN,
                    'transaction_type_id' => TransactionTypeEnum::TYPE_SELL_MINING,
                    'order_id' => 0,
                    'point' => $resultSellBackRelationResidualValue,
                    'before_balance' => $before_balance,
                    'after_balance' => $after_balance,
                    'status' => 1,
                ];
                $resultTransactionLogId = TransactionLog::insertGetId($transactionLog);
                if (!$resultTransactionLogId) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
                //MemberMinerTransactionLog
                $memberMinerTransactionLog = [];
                $memberMinerTransactionLog[] = [
                    'transaction_log_id' => $resultTransactionLogId,
                    'member_miner_id' => $relationMemberMiner->id,
                ];
                $resultMemberMinerTransactionLog = MemberMinerTransactionLog::insert($memberMinerTransactionLog);
                if (!$resultMemberMinerTransactionLog) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
            } else {
                //當前單加上取消時間為當前 改狀態為3
                $memberMinersUpdateData = [
                    'id' => $request->mm_id,
                    'cancel_time' => $now,
                    'status' => 3,
                ];
                $resultSellBackMain = MemberMiner::upsert($memberMinersUpdateData, ['id'], ['cancel_time', 'status']);
                if (!$resultSellBackMain) {
                    throw new Exception(__('error_message.2001'), 2001);
                }
            }
            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取返佣列表
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *         "miner_id": 7, 礦機ID
     *         "miner_name": "PS5 Pro", 礦機名稱
     *         "percentage1": "0.0000", 返佣比例1
     *         "percentage2": "0.0200", 返佣比例2
     *         "percentage3": "0.0100"  返佣比例3
     *       },
     *     ]
     *   }
     * }
     */
    public function getRebateList(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            $minerRepository = new MinerRepository();
            $list = $minerRepository->getRebateList($request);
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


    /**
     * 獲取推薦獎勵列表
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "list": [
     *       {
     *          "miner_id": 1, 礦機ID
     *          "miner_name": "S19k", 礦機名稱
     *          "reward_list": [ 獎勵列表
     *              {
     *                "quantity": 3, 數量
     *                "amount": "20.0000000000" 金額
     *              },
     *              {
     *                "quantity": 5,
     *                "amount": "50.0000000000"
     *              },
     *              {
     *                "quantity": 10,
     *                "amount": "100.0000000000"
     *              }
     *          ]
     *       },
     *     ]
     *   }
     * }
     */
    public function getReferralRewardList(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            $minerService = new MinerService();
            $list = $minerService->getReferralRewardList($request);
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

    //整理礦機列表資料 排除不需要欄位 將info內容組進去後回傳
    private function doMultiMinerData($minerList, $exceptField): array
    {
        $except = array_merge(['promo', 'p_days', 'g_days', 'info', 'created_at', 'updated_at', 'cycle_days', 'type'], $exceptField);
        return collect($minerList)->map(function ($item) use ($except, $exceptField) {
            //贈送租賃
            $promos = [];
            if (!in_array('promo', $exceptField)) {
                $promos = $this->getPromo($item->promo);
                $promos['p_cycle'] = $promos['p_days'] / MinerEnum::CycleDays;
                $promos['g_cycle'] = $promos['g_days'] / MinerEnum::CycleDays;
            }
            //其他資訊
            $infos = [];
            if (!in_array('info', $exceptField)) {
                $infos = json_decode($item->info, true);
            }
            //週期
            if (!empty($item->cycle_days)) {
                $item->cycle = intval($item->cycle_days) / MinerEnum::CycleDays;
            }
            //折舊週期
            if (!empty($item->depreciation_cycle)) {
                $item->depreciation_cycle = $item->depreciation_cycle / MinerEnum::CycleDays;
            }
            //團購需要將購買價格跟收益按比例計算
            if (2 == ($item->type ?? -1)) {
                $item->purchase_price = bcmul($item->purchase_price, MinerEnum::GroupProportion, 4);
                $item->day_revenue = bcmul($item->day_revenue, MinerEnum::GroupProportion, 4);
            }
            return collect($item)->merge($promos)->merge($infos)->except($except);
        })->sortByDesc('sort')->values()->toArray();
    }

    //礦機相關時間 先用+8時區計算後轉UTC+0
    private function getMinerTimesUtc($cycleDays = 0): array
    {
        $nowMy = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend']);
        //開始時間start = 隔天00:00:00
        $startMy = Carbon::parse($nowMy)->addDay()->startOfDay();
        //結束時間end = 今天23:59:59 +3年
        $endMy = Carbon::parse($nowMy)->addYears(3)->endOfDay();
        //可售回時間 can_sellback = 今天23:59:59 + 28天
        $canSellBackMy = Carbon::parse($nowMy)->addDays(28)->endOfDay();
        //若有週期 按周期天數計算
        if (!empty($cycleDays ?? 0)) {
            $endMy = Carbon::parse($startMy)->addDays(($cycleDays - 1))->endOfDay();
            $canSellBackMy = Carbon::parse($startMy)->addDays(($cycleDays - 1))->endOfDay();
        }
        return [
            'start' => Carbon::parse($startMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
            'end' => Carbon::parse($endMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
            'can_sellback' => Carbon::parse($canSellBackMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString(),
        ];
    }

    //購買或團購礦機可租數量及週期
    private function getPromo($promoJson = ''): array
    {
        $promoData = [];
        if (!empty($promoJson)) {
            $promoData = json_decode($promoJson, true);
        }
        return [
            'p_num' => $promoData['purchase']['num'] ?? 0,
            'p_days' => $promoData['purchase']['days'] ?? 0,
            'g_num' => $promoData['group']['num'] ?? 0,
            'g_days' => $promoData['group']['days'] ?? 0,
            'r_ec' => $promoData['rent']['experience_commission'] ?? '0.00',
        ];
    }
}
