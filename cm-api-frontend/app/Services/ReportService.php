<?php

namespace App\Services;

use App\Enums\PromotionEnum;
use App\Helpers\StringHelper;
use App\Repositories\TransactionLogRepository;
use App\Repositories\MemberRepository;
use App\Repositories\MemberMinerRebateRepository;
use App\Repositories\MemberMinerRewardRepository;
use Carbon\Carbon;
use Exception;

class ReportService extends BaseService
{
    public function getFundList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 20;
        $params->type = empty($params->type ?? 0) ? '' : $params->type;

        $repository = new TransactionLogRepository();
        $model = $repository->getFundList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            $item->transaction_type_name = __('param.transaction_type.' . $item->transaction_type_id);
            $item->point = $item->after_balance < $item->before_balance ? ($item->point * -1) : $item->point;
            $item->remark = in_array($item->transaction_type_id, [16]) ? $item->remark : '';
            return $item;
        });
        return [$total, $result];
    }

    public function getRewardList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 20;

        $repository = new MemberMinerRewardRepository();
        $model = $repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y/m/d');
            return $item;
        });
        return [$total, $result];
    }

    public function getRebateList($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 20;

        $repository = new MemberMinerRebateRepository();
        $model = $repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $memberRepository = new MemberRepository();
        $result = $result->map(function ($item) use ($memberRepository) {
            //DB::raw("'bi****51 / ho****86 / rs****42 / an****83' as account"),
            $members = $memberRepository->getAccountByMemberIds($item->member_ids);
            $item->account = '';
            foreach ($members as $member) {
                $item->account .= ($item->account != '' ? ' / ' : '')
                    . StringHelper::stringEncrypt($member->username, 2, 2, 4);
            }

            //'Price 40.10 x Rebate percentage 10%' as info
            $item->info = 'Price ' . $item->price . ' x Rebate percentage ' . ($item->percentage * 100) . '%';
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y/m/d');

            return $item;
        });

        return [$total, $result];
    }
}
