<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Announcement;
use App\Helpers\Common;
use DB;

/**
 * Class AnnouncementRepository.
 */
class AnnouncementRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Announcement::class;
    }

    public function getList($params)
    {
        $model = DB::table('announcements as a')
            ->when(Common::IsNotNullOrEmptyString($params->start_time), function ($query) use ($params) {
                $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
                $query->where('a.start_time', '<=', $start_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->start_time), function ($query) use ($params) {
                $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
                $query->where('a.end_time', '>=', $end_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('a.status', $params->status);
            })
            ->when($params->type > 0, function ($query) use ($params) {
                $query->where('a.type', $params->type);
            })
            ->orderBy('a.id', 'desc')
            ->select(
                'a.id as announcement_id',
                'a.type as type',
                'a.title',
                //'a.content',
                DB::raw("CASE WHEN length(a.content) > 20 THEN CONCAT(SUBSTRING(a.content, 1, 20), '...') ELSE a.content end as contentTxt"),
                'a.content',
                'a.image',
                'a.redirect_url',
                'a.is_top',
                'a.start_time',
                'a.end_time',
                'a.status'
            );
        return $model;
    }
}
