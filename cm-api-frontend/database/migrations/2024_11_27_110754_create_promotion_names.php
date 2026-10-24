<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'promotion_names';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('language_id')->comment('語言系統編號');
                $table->integer('promotion_id')->comment('類型系統編號');
                $table->string('title', 191)->collation('utf8mb4_unicode_ci')->comment('活動標題');
                $table->text('content')->collation('utf8mb4_unicode_ci')->comment('活動內容');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('活動圖片');
                $table->text('url')->collation('utf8mb4_unicode_ci')->comment('跳轉連結');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('language_id', 'idx_' . $this->table_name . '_' . 'language_id');
                $table->index('promotion_id', 'idx_' . $this->table_name . '_' . 'promotion_id');
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
