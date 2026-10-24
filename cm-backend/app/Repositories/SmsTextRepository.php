<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\SmsText;
use DB;

/**
 * Class SmsTextRepository.
 */
class SmsTextRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return SmsText::class;
    }

    public function getText($agent_group_id, $type, $country_id)
    {
        return $this->model->where('agent_group_id', $agent_group_id)
            ->where('type_id', $type)
            ->whereIn('country_id', [1, $country_id])
            ->orderBy('country_id', 'desc')
            ->first();
    }
}
