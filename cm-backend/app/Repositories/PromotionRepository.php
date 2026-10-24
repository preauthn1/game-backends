<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Promotion;
use App\Helpers\Common;
use DB;

/**
 * Class PromotionRepository.
 */
class PromotionRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Promotion::class;
    }

    public function getList($params)
    {
        $model = DB::table('promotions as p')
            ->where('p.miner_group_id', $params->MINER_GROUP)
            ->when(Common::IsNotNullOrEmptyString($params->start_time), function ($query) use ($params) {
                $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
                $query->where('p.start_time', '<=', $start_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->start_time), function ($query) use ($params) {
                $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
                $query->where('p.end_time', '>=', $end_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('p.status', $params->status);
            })
            ->when($params->type > -1, function ($query) use ($params) {
                $query->where('p.type', $params->type);
            })
            ->orderBy('p.sort', 'desc')
            ->orderBy('p.id', 'desc')
            ->select(
                'p.id as promotion_id',
                'p.type',
                'p.miner_group_id',
                'p.title',
                'p.content',
                'p.image',
                'p.url',
                'p.start_time',
                'p.end_time',
                'p.status',
                'p.created_at',
                'p.updated_at',
            );
        return $model;
    }
}
