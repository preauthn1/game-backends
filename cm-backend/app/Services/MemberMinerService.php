<?php

namespace App\Services;

use App\Enums\MinerEnum;
use App\Enums\TimeEnum;
use App\Helpers\MinerHelper;
use App\Models\MemberMiner;
use App\Repositories\MemberRepository;
use App\Repositories\MinerRepository;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Hash;
use App\Helpers\Common;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MemberWalletRepository;
use App\Repositories\MemberBankAccountRepository;
use App\Repositories\MemberCryptoAccountRepository;
use App\Enums\WalletTypeEnum;
use Exception;

class MemberMinerService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberMinerRepository();
    }

    public function getWaitingList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getWaitingList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->typeTxt = __('param.miner.type.' . $item->type);
            $item->memberTypeTxt = __('param.member.type.' . $item->member_type);
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');

            return $item;
        });
        return [$total, $result];
    }

    public function getWaiting($mm_id)
    {
        $waiting = $this->repository->getWaiting($mm_id);
        if (!$waiting) {
            throw new Exception(__('error_message.9001'), 9001);
        }

        $waiting->memberTypeTxt = __('param.member.type.' . $waiting->member_type);
        $waiting->created_at = Carbon::parse($waiting->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');

        return $waiting;
    }

    public function createWaiting($params)
    {
        $minerRepository = new MinerRepository();
        $miner = $minerRepository->findByName($params->minerName, $params->MINER_GROUP);
        if (!is_object($miner)) {
            throw new Exception(__('error_message.5034'), 5034);
        }

        $memberRepository = new MemberRepository();
        $member = $memberRepository->findByUsername($params->username, $params->HIERARCHY_PATH);
        if (!is_object($member)) {
            throw new Exception(__('error_message.5007'), 5007);
        }
        //非測試帳號
        if (!in_array($member->type, [2, 3])) {
            throw new Exception(__('error_message.5029'), 5029);
        }

        $data = array();
        $data['type'] = MinerEnum::MinerType['group'];
        $data['member_id'] = $member->id;
        $data['miner_id'] = $miner->id;
        $data['status'] = 0;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateWaiting($params)
    {
        $memberRepository = new MemberRepository();
        $memberTest = $memberRepository->findByUsername($params->testUsername);
        if (!is_object($memberTest)) {
            throw new Exception(__('error_message.5007'), 5007);
        }
        //非測試帳號
        if (!in_array($memberTest->type, [2, 3])) {
            throw new Exception(__('error_message.5029'), 5029);
        }
        //團購礦機資訊
        $memberMiners = MemberMiner::find($params->mm_id);
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
        //礦機資訊
        $minerService = new MinerService();
        $miner = $minerService->getById($memberMiners->miner_id);
        //礦機相關時間
        $minerHelper = new MinerHelper();
        $minerTimes = $minerHelper->getMinerTimesUtc();

        DB::beginTransaction();

        //更新開團單
        $repository = new MemberMinerRepository();
        $data = array();
        $data['parent_id'] = $params->mm_id;
        $data['check_time'] = Carbon::now();
        $data['start_time'] = $minerTimes['start'];
        $data['end_time'] = $minerTimes['end'];
        $data['can_sellback_time'] = $minerTimes['can_sellback'];
        $data['status'] = 1;
        $resultUpdateGroup = $repository->updateById($params->mm_id, $data);
        //團購礦機可租數量
        $promos = $minerHelper->getPromo($miner->promo);
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
            'member_id' => $memberTest->id,
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
                'member_id' => $memberTest->id,
                'miner_id' => $memberMiners->miner_id,
                'parent_id' => $resultInsertGroupId,
                'status' => 0,
                'info' => '{"cycle_days":' . ($promos['g_days'] ?? 0) . '}'
            ];
        }
        $resultRent = MemberMiner::insert($memberMinerRent);
        if (!$resultUpdateGroup || !$resultInsertGroupId || !$resultRent) {
            DB::rollback();
            throw new Exception(__('error_message.2001'), 2001);
        }
        DB::commit();
        return response()->json([
            'code' => 1000,
            'message' => __('error_message.1000')
        ]);
    }

    public function cancelWaiting($params)
    {
        $repository = new MemberMinerRepository();
        $data = array();
        $data['end_time'] = Carbon::now();
        $data['cancel_time'] = Carbon::now();
        $data['status'] = 2;
        $result = $repository->updateById($params->mm_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getPurchaseList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getPurchaseList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->statusTxt = __('param.purchase_order.status.' . $item->status);
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            if(!empty($item->check_time??'')){
                $item->check_time = Carbon::parse($item->check_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function getRentList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getRentList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->statusTxt = __('param.rent_order.status.' . $item->status);
            $item->isExperience = __('param.isExperience.' . $item->isExperience);
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            if (!empty($item->start_time ?? '')) {
                $item->start_time = Carbon::parse($item->start_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            }
            if (!empty($item->end_time ?? '')) {
                $item->end_time = Carbon::parse($item->end_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            }
            $item->cycle_days = intval($item->cycle_days ?? 0) / MinerEnum::CycleDays;
            if(!empty($item->check_time??'')){
                $item->check_time = Carbon::parse($item->check_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function getGroupList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getGroupList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->statusTxt = __('param.group_order.status.' . $item->status);
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            if(!empty($item->check_time??'')){
                $item->check_time = Carbon::parse($item->check_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function createExperience($params)
    {
        $data = array();
        $data['type'] = 0;
        $data['member_id'] = $params->member_id;
        $data['miner_id'] = $params->miner_id;
        $data['status'] = 0;
        $data['info'] = $params->info;
        $data['operator'] = $params->operator;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
