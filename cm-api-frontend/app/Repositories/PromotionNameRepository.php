<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\PromotionName;
use DB;

/**
 * Class PromotionNameRepository.
 */
class PromotionNameRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return PromotionName::class;
    }
}
