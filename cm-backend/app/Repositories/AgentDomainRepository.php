<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentDomain;

/**
 * Class AgentDomainRepository.
 */
class AgentDomainRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return AgentDomain::class;
    }

    public function getByDomain($domain)
    {
        return $this->model->where('domain', $domain)
            ->where('status', 1)
            ->first();
    }
}
