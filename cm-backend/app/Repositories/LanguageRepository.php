<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Language;

/**
 * Class LanguageRepository.
 */
class LanguageRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Language::class;
    }

    public function getByCode($code)
    {
        return $this->model->where('code', $code)
            ->where('status', 1)
            ->first();
    }

    public function getDropdown()
    {
        return $this->model
            ->where('status', 1)
            ->select(
                'id',
                'name',
                'code'
            )
            ->orderBy('sort')
            ->get();
    }
}
