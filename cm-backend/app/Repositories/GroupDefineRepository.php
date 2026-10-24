<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentSettingDefine;
use Illuminate\Support\Facades\DB;

class GroupDefineRepository extends BaseRepository
{
    public function model()
    {
        return AgentSettingDefine::class;
    }

    public function findByDefineId($id)
    {
        return $this->model->where('define_id', $id);
    }

    public function getList($params)
    {
        return $this->model
            ->when(Common::IsNotNullOrEmptyString($params->parent_id), function ($query) use ($params) {
                $query->where('parent_id', $params->parent_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->group_type), function ($query) use ($params) {
                $query->where('group_type', $params->group_type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->ORGANIZATION_ID), function ($query) use ($params) {
                $query->where('organization_id', $params->ORGANIZATION_ID);
            })
            ->select(
                'define_id',
                'group_type',
                'is_visible',
                'is_required',
                'language_text',
                'default_value',
                'data_type',
                'sort',
                'memo',
                'parent_id',
            )
            ->get();
    }

    public function view($define_id)
    {
        return DB::table('agent_setting_defines as asd')
            ->leftJoin('agent_setting_defines AS asdp', 'asd.parent_id', 'asdp.define_id')
            ->where('asd.define_id', $define_id)
            ->select(
                'asd.*',
                DB::raw("IFNULL(asdp.language_text, '') as parent_language_text"),
            )
            ->first();
    }

    public function updateByDefineId($id, $data)
    {
        return $this->model
            ->where('define_id', $id)
            ->update($data);
    }
}
