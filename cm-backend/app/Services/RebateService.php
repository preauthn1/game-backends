<?php

namespace App\Services;

use Illuminate\Support\Carbon;
use App\Repositories\MemberMinerRebateRepository;
use App\Helpers\Common;
use Exception;
use DB;

class RebateService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberMinerRebateRepository();
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = DB::table(DB::raw('(' . $model->toSql() . ') as z'))
            ->mergeBindings($model)
            ->count();
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
