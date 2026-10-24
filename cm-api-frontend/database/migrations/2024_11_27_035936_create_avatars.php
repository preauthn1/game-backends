<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'avatars';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('頭像圖示');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'organization_id' => 1, 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/crypto_miner/1308845.png', 'status' => 1],
                ['id' => 2, 'organization_id' => 1, 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/crypto_miner/1326390.png', 'status' => 1],
                ['id' => 3, 'organization_id' => 1, 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/crypto_miner/1326405.png', 'status' => 1],
                ['id' => 4, 'organization_id' => 1, 'image' => 'https://example.invalid/cdn-betmonsters-com-cdn-bucket/avatars/crypto_miner/1326415.png', 'status' => 1],
            ]);
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
