<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MiningBlock;
use Illuminate\Support\Facades\DB;

class MiningBlockRepository extends BaseRepository
{
    public function model()
    {
        return MiningBlock::class;
    }

    public function getList($params)
    {
        return DB::table('mining_blocks AS mb')
            ->join("currencies as c", "mb.currency_id", "c.id")
            ->leftJoin("agents as a","mb.operator","a.id")
            ->orderBy("mb.id", "desc")
            ->select(
                'mb.id',
                'c.name as currency_name',
                'mb.block_height',
                'mb.block_time',
                'mb.mining_difficulty',
                'mb.mining_change',
                'mb.block_reward',
                'mb.status',
                DB::raw("IFNULL(a.username, '') as reviewer"),
                'mb.created_at',
                'mb.updated_at'
            );
    }
}
