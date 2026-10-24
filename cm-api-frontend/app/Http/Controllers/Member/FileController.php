<?php

namespace App\Http\Controllers\Member;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Helpers\FileHelper;
use App\Helpers\CloudFlareHelper;
use Validator;
use Exception;

class FileController extends Controller
{
    public function uploadIdInformation(Request $request)
    {
        try {

            //參數驗證
            $validator = Validator::make($request->all(), [
                //'id_front_image' => 'required|image|mimes:jpeg,png,jpg,gif,svg|max:2048',
                //'id_back_image' => 'required|image|mimes:jpeg,png,jpg,gif,svg|max:2048',
                'id_front_image' => [
                    'required',
                    'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                ],
                'id_back_image' => [
                    'required',
                    'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                ],
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $path = 'member/' . $request->ID. '/id_card';
            $front_image = CloudFlareHelper::uploadBase64File($request->id_front_image, $path);
            $back_image = CloudFlareHelper::uploadBase64File($request->id_back_image, $path);
            $service = new MemberService();
            $service->createIdInformation($request->ID, $front_image, $back_image);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'id_card_front' => $front_image,
                    'id_card_back' => $back_image
                ]
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2],
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        }
    }
}
