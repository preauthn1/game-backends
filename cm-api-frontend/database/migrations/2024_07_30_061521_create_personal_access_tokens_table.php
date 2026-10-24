<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $table_name = 'personal_access_tokens';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->bigIncrements('id');
                $table->string('tokenable_type', 191)->collation('utf8mb4_unicode_ci');
                $table->unsignedBigInteger('tokenable_id');
                $table->string('name', 191)->collation('utf8mb4_unicode_ci');
                $table->string('token', 64)->collation('utf8mb4_unicode_ci');
                $table->text('abilities')->collation('utf8mb4_unicode_ci');
                $table->timestamp('last_used_at')->nullable();
                $table->timestamp('expires_at')->nullable();
                $table->timestamp('created_at')
                    ->useCurrent();
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'));

                //unique
                $table->unique('token', 'personal_access_tokens_token_unique');
                //index
                $table->index(['tokenable_type', 'tokenable_id'], 'personal_access_tokens_tokenable_type_tokenable_id_index');
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
