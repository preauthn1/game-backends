<?php

namespace App\Services;

use App\Repositories\AgentDomainRepository;
use Exception;

/**
 * Class AgentDomainService.
 */
class AgentDomainService extends BaseService
{
    public function getByDomain($domain)
    {
        $this->repository = new AgentDomainRepository();
        $result = $this->repository->getByDomain($domain);
        if (!is_object($result)) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getAgentIdByDomain($domain)
    {
        $agentDomain = $this->getByDomain($domain);
        return $agentDomain->agent_id;
    }
}
