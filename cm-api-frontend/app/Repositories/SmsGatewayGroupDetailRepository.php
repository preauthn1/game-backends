<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsGatewayGroupDetail;
use DB;

/**
 * Class SmsGatewayGroupDetailRepository.
 */
class SmsGatewayGroupDetailRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsGatewayGroupDetail::class;
    }

    public function getSetting($agent_group_id)
    {
        return DB::table('sms_gateway_group_details as sggd')
            ->where('sggd.agent_group_id', $agent_group_id)
            ->select(
                'sggd.id',
                'sggd.setting'
            )
            ->first();
    }

}
