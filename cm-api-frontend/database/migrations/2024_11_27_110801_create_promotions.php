<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'promotions';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->tinyInteger('type')->default(0)->comment('類型; 0:預設(跳轉url), 1:登入頁彈窗, 2:內頁彈窗');
                $table->integer('miner_group_id')->nullable()->comment('礦機群組編號');
                $table->string('title', 191)->collation('utf8mb4_unicode_ci')->comment('活動標題');
                $table->text('content')->collation('utf8mb4_unicode_ci')->comment('活動內容');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('活動圖案');
                $table->text('url')->collation('utf8mb4_unicode_ci')->comment('跳轉連結');
                $table->text('image_content')->collation('utf8mb4_unicode_ci')->comment('活動內容');
                $table->dateTime('start_time')->nullable();
                $table->dateTime('end_time')->nullable();
                $table->integer('sort')->default(0)->comment('排序, 大到小');
                $table->tinyInteger('status')->default(0)->comment('狀態 0:停用,1:啟用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('start_time', 'idx_' . $this->table_name . '_' . 'start_time');
                $table->index('end_time', 'idx_' . $this->table_name . '_' . 'end_time');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
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
