<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\WalletType;
use DB;

/**
 * Class WalletTypeRepository.
 */
class WalletTypeRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return WalletType::class;
    }

    public function getByLanguageId($language_id)
    {
        $model = DB::table('wallet_types as wt')
            ->leftJoin('wallet_type_names as wtn', function ($join) use ($language_id) {
                $join->on('wtn.wallet_type_id', 'wt.id')
                    ->where('language_id', $language_id);
            })
            ->where('status', 1)
            ->orderBy('wt.id', 'ASC')
            ->select(
                'wt.id',
                DB::raw('IFNULL(wtn.name, wt.name) AS name')
            )
            ->get();
        return $model;
    }
}
