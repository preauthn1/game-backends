<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\AgentGroupRepository;
use App\Repositories\AgentSettingRepository;
use App\Repositories\GroupDefineRepository;
use Exception;
use Illuminate\Support\Facades\DB;

class AgentSettingService extends BaseService
{

    public function __construct()
    {
        $this->repository = new AgentSettingRepository();
    }

    public function createDefaultSetting($params)
    {
        $agentGroupRepository = new AgentGroupRepository();
        $groupData = $agentGroupRepository->getById($params->groupId);
        if (!$groupData) {
            throw new Exception(__('error_message.9001'), 9001);
        }

        $groupDefineRepository = new GroupDefineRepository();
        $defineData = $groupDefineRepository->getList($params);
        $insertDefine = [];
        foreach ($defineData ?? [] as $data) {
            if (0 != $data->parent_id) {
                $insertDefine[] = [
                    'agent_group_id' => $params->groupId,
                    'setting_key' => $data->define_id,
                    'value' => $data->default_value
                ];
            }
        }
        $result = DB::table('agent_customized_settings')->insert($insertDefine);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
