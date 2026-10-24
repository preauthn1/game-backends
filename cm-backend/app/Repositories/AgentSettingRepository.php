<?php

namespace App\Repositories;

use App\Helpers\Common;
use Illuminate\Support\Facades\DB;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentCustomizedSetting;

class AgentSettingRepository extends BaseRepository
{
    public function model()
    {
        return AgentCustomizedSetting::class;
    }

    public function getGroupSettingIdList($params)
    {
        return DB::table('agent_groups as ag')
            ->leftJoin('agent_customized_settings as acs', function ($join) use ($params) {
                $join->on('ag.id', 'acs.agent_group_id')
                    ->where('acs.setting_key', $params->define_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('ag.organization_id', $params->ORGANIZATION_ID);
            })
            ->when(Common::IsNotNullOrEmptyString($params->group_type), function ($query) use ($params) {
                $query->where('ag.type', $params->group_type);
            })
            ->select(
                'ag.id as group_id',
                'acs.id as setting_id'
            );
    }
}
