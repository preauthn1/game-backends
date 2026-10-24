<?php

namespace App\Services;

use App\Repositories\AgentRepository;
use Exception;

/**
 * Class AgentService.
 */
class AgentService extends BaseService
{
    public function find($agent_id, $check_status = false)
    {
        $this->repository = new AgentRepository();
        $agent = $this->repository->find($agent_id);
        //檢查帳號是否存在
        if (!is_object($agent)) {
            throw new Exception(__('error_message.9998'), 9998);
        }
        //檢查帳號是否鎖定
        if ($check_status && $agent->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }
        return $agent;
    }

    public function getByReferralCode($code)
    {
        $this->repository = new AgentRepository();
        return $this->repository->getByReferralCode($code);
    }

    public function getCustomizedSetting(object $params)
    {
        if (empty($params->setting_key)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $this->repository = new AgentRepository();
        return $this->repository->getCustomizedSettings($params)->first();
    }

    public function getCustomizedSettings(object $params)
    {
        if (empty($params->setting_keys) || !is_array($params->setting_keys)) {
            throw new Exception(__('error_message.9001'), 9001);
        }

        $repository = new AgentRepository();
        return $repository->getCustomizedSettings($params)->get();
    }
}
