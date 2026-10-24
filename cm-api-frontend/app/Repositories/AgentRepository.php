<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Agent;
use App\Helpers\Common;
use DB;

/**
 * Class AgentRepository.
 */
class AgentRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Agent::class;
    }

    public function find($agent_id)
    {
        return $this->model->find($agent_id);
    }

    public function getByReferralCode($code)
    {
        return $this->model->where('code', $code)->first();
    }

    public function getCustomizedSettings($params)
    {
        return DB::table('agent_customized_settings')
            ->where('agent_group_id', $params->CUSTOMIZED_SETTING_GROUP)
            ->when(Common::IsNotNullOrEmptyString($params->setting_key), function ($query) use ($params) {
                $query->where('setting_key', $params->setting_key);
            })
            ->when(!empty($params->setting_keys ?? []), function ($query) use ($params) {
                $query->whereIn('setting_key', $params->setting_keys);
            });
    }
}
