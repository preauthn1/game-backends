<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Announcement;
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

    public function getList($member_id, $type)
    {
        $now = date('Y-m-d H:i:s');
        $model = DB::table('announcements as a')
            ->leftJoin('announcement_reads as ar', function ($join) use ($member_id) {
                $join->on('ar.announcement_id', 'a.id')
                    ->where('ar.member_id', $member_id);
            })
            ->where('a.start_time', '<=', $now)
            ->where('a.end_time', '>', $now)
            ->when($type > 0, function ($query) use ($type) {
                $query->where('a.type', $type);
            })
            ->where('a.status', 1)
            ->orderBy('a.type', 'asc')
            ->orderBy('a.is_top', 'desc')
            ->orderBy('a.id')
            ->select(
                'a.id as announcement_id',
                'a.type as announcement_type',
                DB::raw(
                    "case a.type when 1 then '" . __('param.announcement.type.1') . "' " .
                    "when 2 then '" . __('param.announcement.type.2') . "' " .
                    "when 3 then '" . __('param.announcement.type.3') . "' " .
                    "else 'unknow' end as announcement_type_text"
                ),
                'a.title as announcement_title',
                'a.content as announcement_content',
                DB::raw('DATE_ADD(a.start_time, INTERVAL 8 HOUR) AS start_time'),
                DB::raw('DATE_ADD(a.end_time, INTERVAL 8 HOUR) AS end_time'),
                DB::raw('CASE WHEN ar.id = null THEN 0 ELSE 1 END AS is_read')
            )
            ->get();
        return $model;
    }
}
