<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberMinerDepreciation;
use DB;

/**
 * Class MemberMinerDepreciationRepository.
 */
class MemberMinerDepreciationRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberMinerDepreciation::class;
    }

    public function getResidualValue($id)
    {
        return $this->model->where('member_miner_id', $id)
            ->orderBy('id', 'desc')
            ->select(
                'id',
                'member_miner_id',
                'mining_block_id',
                DB::raw('TRUNCATE(residual_value, 2) AS residual_value'),
                DB::raw('TRUNCATE(depreciation_amount, 2) AS depreciation_amount'),
                DB::raw('TRUNCATE(residual_value, 2) AS next_residual_value'),
                DB::raw('TRUNCATE(next_depreciation_amount, 2) AS next_depreciation_amount'),
                'info'
            )
            ->first();
    }
}
