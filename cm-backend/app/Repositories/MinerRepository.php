<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Miner;
use App\Enums\MinerEnum as MinerEnum;
use App\Helpers\Common;
use Illuminate\Support\Facades\DB;

class MinerRepository extends BaseRepository
{
    public function model(): string
    {
        return Miner::class;
    }

    /**
     * 取得礦機機型
     *
     * @param array $params 條件
     * @return array
     */
    public function getDropdown($params, $status = -1)
    {
        return $this->model
            ->where('agent_group_id', $params->MINER_GROUP)
            ->when($status >= 0, function ($query) use ($status) {
                $query->where('status', $status);
            })
            ->select(
                'id as miner_id',
                'name as miner_name',
                'image as miner_image',
                'promo as miner_promo',
                'rent_price'
            );
    }

    public function findByName($name, $agent_group_id = 0)
    {
        return $this->model->where('name', $name)
            ->when($agent_group_id > 0, function ($query) use ($agent_group_id) {
                $query->where('agent_group_id', $agent_group_id);
            })
            ->first();
    }

    public function getMiner($id)
    {
        return DB::table('miners as miner')
            ->leftJoin('miner_rebates as rebate', 'miner.id', 'rebate.miner_id')
            ->leftJoin('miner_rewards as reward', 'miner.id', 'reward.miner_id')
            ->where('miner.id', $id)
            ->select(
                'miner.id',
                'miner.name',
                'miner.image',
                'miner.currency_id',
                'miner.hash_rate',
                'miner.power_consumption',
                'miner.day_revenue',
                'miner.depreciation_cycle',
                'miner.purchase_price',
                'miner.purchase_total',
                'miner.promo',
                'miner.sort',
                'miner.is_show',
                'miner.status',
                'example.invalid',
                'miner.rent_price',
                'miner.group_total',
                'rebate.percentage1 as rebate_percentage1',
                'rebate.percentage2 as rebate_percentage2',
                'rebate.percentage3 as rebate_percentage3',
                DB::raw("MAX(CASE WHEN reward.quantity = 3 THEN reward.amount ELSE NULL END) as reward_quantity_3"),
                DB::raw("MAX(CASE WHEN reward.quantity = 5 THEN reward.amount ELSE NULL END) as reward_quantity_5"),
                DB::raw("MAX(CASE WHEN reward.quantity = 10 THEN reward.amount ELSE NULL END) as reward_quantity_10")
            )
            ->groupBy('miner.id')
            ->first();
    }

    public function getList($params)
    {
        return DB::table('miners as miner')
            ->join('currencies as c', 'c.id', 'miner.currency_id')
            ->where('miner.agent_group_id', $params->MINER_GROUP)
            ->when(Common::IsNotNullOrEmptyString($params->name), function ($query) use ($params) {
                $query->where('miner.name', 'like', '%' . $params->name . '%');
            })
            ->when(isset($params->is_show), function ($query) use ($params) {
                $query->where('miner.is_show', $params->is_show);
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('miner.status', $params->status);
            })
            ->orderBy('miner.sort', 'desc')
            ->select(
                'miner.id as miner_id',
                'miner.name',
                'miner.image',
                'miner.currency_id',
                'c.name as currency_name',
                'miner.hash_rate',
                'miner.power_consumption',
                'miner.day_revenue',
                DB::raw("miner.depreciation_cycle div ".MinerEnum::CycleDays." as depreciation_cycle"),
                'miner.purchase_price',
                'miner.purchase_total',
                'miner.rent_price',
                'miner.rent_total',
                'miner.group_price',
                'miner.group_total',
                'miner.promo',
                'miner.sort',
                'miner.is_show',
                'miner.status',
                'example.invalid',
                DB::raw('DATE_ADD(miner.created_at, INTERVAL 8 HOUR) as created_at'),
                DB::raw('DATE_ADD(miner.updated_at, INTERVAL 8 HOUR) as updated_at')
            );
    }
}
