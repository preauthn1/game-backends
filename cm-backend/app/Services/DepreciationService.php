<?php

namespace App\Services;

use App\Repositories\DepreciationRepository;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Carbon;

class DepreciationService extends BaseService
{
    public function __construct()
    {
        $this->repository = new DepreciationRepository();
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
            $item->memberTypeTxt = __('param.member.type.' . $item->member_type);
            $item->typeTxt = __('param.miner.type.' . $item->type);
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d');
            return $item;
        });
        return [$total, $result];
    }
}
