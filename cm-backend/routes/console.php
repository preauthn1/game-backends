<?php

use Illuminate\Foundation\Inspiring;
use Illuminate\Support\Facades\Artisan;
use App\Models\Task;
use Illuminate\Support\Facades\Schedule;
use Carbon\Carbon;

// Artisan::command('inspire', function () {
//     $this->comment(Inspiring::quote());
// })->purpose('Display an inspiring quote')->hourly();

$tasks = Task::where('status', 1)->get();
foreach ($tasks as $task) {
    Schedule::command($task->command)->cron($task->expression)->before(function () use ($task) {
        Task::find($task->id)->update(['last_run_at' => Carbon::now()]);
    });
}
