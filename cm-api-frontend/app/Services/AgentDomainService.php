<?php

namespace App\Services;

use App\Repositories\AgentDomainRepository;
use Exception;

/**
 * Class AgentDomainService.
 */
class AgentDomainService extends BaseService
{
    public function __construct()
    {
        $this->repository = new AgentDomainRepository();
    }

    public function getByDomain($domain)
    {
        $result = $this->repository->getByDomain($domain);
        if (!is_object($result)) {
            throw new Exception(__('error_message.5033') . '(' . $domain . ')', 5033);
        }
        return $result;
    }

    public function getAgentIdByDomain($domain)
    {
        $agentDomain = $this->getByDomain($domain);
        return $agentDomain->agent_id;
    }
}
