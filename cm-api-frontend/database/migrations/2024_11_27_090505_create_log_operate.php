<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'log_operate';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('module', 50)->collation('utf8mb4_unicode_ci')->comment('操作');
                $table->string('action', 50)->collation('utf8mb4_unicode_ci')->comment('操作');
                $table->text('params')->collation('utf8mb4_unicode_ci')->comment('参数');
                $table->string('ip', 255)->collation('utf8mb4_unicode_ci')->comment('ip 地址');
                $table->string('http_method', 10)->collation('utf8mb4_unicode_ci')->comment('http 请求方式');
                $table->smallInteger('http_code')->comment('http status code');
                $table->unsignedInteger('start_at')->comment('请求开始时间');
                $table->smallInteger('time_taken')->comment('请求消耗时间/ms');
                $table->unsignedInteger('creator_id')->default(0)->comment('creator id');
                $table->unsignedInteger('created_at')->default(0)->comment('created time');
            });
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
