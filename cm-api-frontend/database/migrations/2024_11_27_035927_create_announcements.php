<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'announcements';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->tinyInteger('type')->comment('公告類別，1:system, 2:promotion, 3:platform');
                $table->string('title', 100)->collation('utf8mb4_unicode_ci')->comment('公告標題');
                $table->text('content')->collation('utf8mb4_unicode_ci')->comment('公告內容');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('圖片');
                $table->text('redirect_url')->collation('utf8mb4_unicode_ci')->comment('跳轉網址');
                $table->tinyInteger('is_top')->default(0)->comment('是否置頂, 0:否, 1:是');
                $table->dateTime('start_time')->nullable();
                $table->dateTime('end_time')->nullable();
                $table->tinyInteger('status')->default(0);
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('type', 'idx_' . $this->table_name . '_' . 'type');
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
