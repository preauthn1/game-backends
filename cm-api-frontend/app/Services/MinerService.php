<?php

namespace App\Services;

use Carbon\Carbon;
use App\Helpers\Common;
use App\Helpers\StringHelper;
use App\Repositories\MemberRepository;
use App\Repositories\MinerRepository;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MemberSalesRankingRepository;
use Exception;

class MinerService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MinerRepository();
    }

    public function getById($id)
    {
        return $this->repository->getById($id);
    }

    public function getSalesList($params)
    {
        $member_ids = array();
        $memberRepository = new MemberRepository();
        $member = $memberRepository->find($params->ID);
        $invite_code = array();
        $invite_code[] = $member->invitation_code;
        for ($level = 1; $level < 3; $level++) {
            if (count($invite_code) > 0) {
                $teams = $memberRepository->getTeams($invite_code);
                $invite_code = array();
                foreach ($teams as $row) {
                    $member_ids[] = $row->id;
                    $invite_code[] = $row->invitation_code;
                }
            }
        }

        $repository = new MemberMinerRepository();
        $miner_id = Common::IsNotNullOrEmptyString($params->miner_id) ? $params->miner_id : 0;
        $result = $repository->getSalesList($member_ids, $miner_id);

        $sales = $result->map(function ($item) {
            $item->username = StringHelper::stringEncrypt($item->username, 2, 2, 4);
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            return $item;
        });
        return $sales;
    }

    public function getTopSelling()
    {
        $result = $this->repository->getTopSelling('hot', 3);
        $result = $result->map(function ($item) {
            $item->info = $item->hash_rate . 'T'; //56T 是哈希率，表示计算能力为 56 TH/s。
            $item->info .= ' / '  . $item->power_consumption . 'W';  //2250W 是功耗，表示电力消耗为 2250 瓦。
            //$item->info .= ' / '  . round(($item->power_consumption / $item->hash_rate), 2) . 'J/T';  //45J/T 是能效比，表示每 TeraHash 需要 45 焦耳 的电力
            return $item;
        });
        return $result;
    }

    public function getSalesRanking($request)
    {
        $page = 1;
        $per_page = 10;
        $repository = new MemberSalesRankingRepository();
        $model = $repository->getList($request);
        $result = $model->orderBy('rebate_amount', 'desc')
            ->orderBy('quantity_sold', 'desc')
            ->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->username = StringHelper::stringEncrypt($item->username, 2, 2, 4);
            return $item;
        });
        return $result;
    }

    /**
     * 獲取推薦獎勵列表
     */
    public function getReferralRewardList($params): array
    {

        $minerRepository = new MinerRepository();
        $list = $minerRepository->getReferralRewardList($params);
        $groupedList = collect($list)
            ->groupBy('miner_id')
            ->map(fn($items) => [
                'miner_id' => $items->first()->miner_id,
                'miner_name' => $items->first()->miner_name,
                'reward_list' => $items->map(fn($item) => [
                    'quantity' => $item->quantity,
                    'amount' => $item->amount
                ])->values()->toArray()
            ])
            ->values()
            ->toArray();

        return $groupedList;
    }
}
