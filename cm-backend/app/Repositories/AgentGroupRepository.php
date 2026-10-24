<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentGroup;
use Illuminate\Support\Facades\DB;

class AgentGroupRepository extends BaseRepository
{
    public function model()
    {
        return AgentGroup::class;
    }

    public function getTypeDropdown($params)
    {
        return $this->model
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('organization_id', $params->ORGANIZATION_ID);
            })
            ->select(
                'type as type_id',
            )
            ->groupBy('type')
            ->get();
    }

    public function getDropdown($params)
    {
        return $this->model
            ->when(Common::IsNotNullOrEmptyString($params->type), function ($query) use ($params) {
                $query->where('type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('organization_id', $params->ORGANIZATION_ID);
            })
            ->select(
                'id',
                'type',
                'name',
            )
            ->orderBy('id')
            ->get();
    }

    public function getList($params)
    {
        return DB::table('agent_setting_defines AS asd')
            ->join('agent_setting_defines AS asdp','asd.parent_id', 'asdp.define_id')
            ->leftJoin('agent_customized_settings AS acs', function ($join) use ($params) {
                $join->on('asd.define_id', 'acs.setting_key')
                    ->when(Common::IsNotNullOrEmptyString($params->group_id), function ($query) use ($params) {
                        $query->where('acs.agent_group_id', $params->group_id);
                    });
            })
            ->when(Common::IsNotNullOrEmptyString($params->group_type), function ($query) use ($params) {
                $query->where('asd.group_type', $params->group_type);
            })
            ->orderBy('asd.parent_id')
            ->select(
                'acs.id',
                'asd.define_id',
                'asd.parent_id',
                'asdp.language_text as parent_txt',
                'asd.group_type',
                'asd.language_text',
                'asd.data_type',
                DB::raw('case when isnull(acs.value) then asd.default_value else acs.value end as value')
            );
    }

    public function view($params)
    {
        return DB::table('agent_setting_defines AS asd')
            ->join('agent_setting_defines AS asdp','asd.parent_id', 'asdp.define_id')
            ->leftJoin('agent_customized_settings AS acs', 'asd.define_id', 'acs.setting_key')
            ->where('acs.id', $params->setting_id)
            ->select(
                'acs.id',
                'asd.define_id',
                'asd.parent_id',
                'asdp.language_text as parent_txt',
                'asd.group_type',
                'asd.language_text',
                'asd.data_type',
                DB::raw('case when isnull(acs.value) then asd.default_value else acs.value end as value'),
                'asd.memo'
            )->first();
    }
}
