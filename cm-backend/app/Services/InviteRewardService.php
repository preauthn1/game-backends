<?php

namespace App\Services;

use App\Repositories\MemberMinerRewardRepository;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Carbon;

class InviteRewardService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberMinerRewardRepository();
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->orderBy('id', 'desc')
            ->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
    }
}
