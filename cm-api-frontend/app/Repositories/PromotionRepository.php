<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Enums\PromotionEnum;
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

    public function getList($miner_group_id, $language_id, $type)
    {
        $now = date('Y-m-d H:i:s');
        $model = DB::table('promotions as p')
            ->leftJoin('promotion_names as pn', function ($join) use ($language_id) {
                $join->on('pn.promotion_id', 'p.id')
                    ->where('pn.language_id', $language_id);
            })
            ->where('p.miner_group_id', $miner_group_id)
            ->where('p.start_time', '<=', $now)
            ->where('p.end_time', '>', $now)
            ->where('p.status', 1)
            ->when(Common::IsNotNullOrEmptyString($type), function ($query) use ($type) {
                $query->where('type', $type);
            })
            ->when(!Common::IsNotNullOrEmptyString($type), function ($query) use ($type) {
                $query->whereIn('type', [PromotionEnum::TYPE_DEFAULT, PromotionEnum::TYPE_IMAGE_CONTENT]);
            })
            ->orderBy('p.sort', 'desc')
            ->orderBy('p.id', 'desc')
            ->select(
                'p.id',
                'p.type',
                DB::raw('IFNULL(pn.title, p.title) as title'),
                DB::raw('IFNULL(pn.content, p.content) as content'),
                DB::raw('IFNULL(pn.image, p.image) as image'),
                DB::raw('IFNULL(pn.url, p.url) as url'),
                'p.image_content',
            )
            ->get();
        return $model;
    }
}
