@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.member.idInformation')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>UID：</label>
                        <input type="text" class="form-control" wire:model="uid"
                               oninput="value=value.replace(/\D/g,'')">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="status">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.operator')}}：</label>
                        <input type="text" class="form-control" wire:model="operator">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="timeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>ID</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>UID</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.operator')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>
                                    @if(0==$data->status)
                                        <button class="btn btn-warning"
                                                wire:click="showDataPop({{$data->member_id_card_id??''}},'modify')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    @else
                                        <button class="btn btn-warning"
                                                wire:click="showDataPop({{$data->member_id_card_id??''}},'detail')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    @endif
                                </td>
                                <td>{{$data->member_id_card_id??''}}</td>
                                <td>{{$data->statusTxt??''}}</td>
                                <td>{{$data->uid??''}}</td>
                                <td>{{$data->username??''}}</td>
                                <td>{{$data->operator??''}}</td>
                                <td>{{$data->created_at??''}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="7">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="7" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <!-- dataPop -->
        <div class="modal fade in" id="idCardPop" role="dialog"
             style="display: {{'idCardPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;margin: 15px 0;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.member.modifyIdInformation'):__('web_message.menu.member.idInformationDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body" style="padding-bottom: 10px;">
                                <input type="hidden" wire:model="modifyId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">UID</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUid" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.username')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUsername" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.image')}}</label>
                                    <div class="col-sm-8">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                @if(''==$popFrontImage)
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;height: 100px;">
                                                        <img class="image-responsive" src="" style="width: 170px;">
                                                    </div>
                                                @else
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;cursor: pointer;height: 100px;"
                                                         onclick="window.open('{{$popFrontImage}}','_blank');">
                                                        <img class="image-responsive" src="{{$popFrontImage}}"
                                                             style="width: 170px;">
                                                    </div>
                                                @endif
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.image')}}</label>
                                    <div class="col-sm-8">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                @if(''==$popBackImage)
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;height: 100px;">
                                                        <img class="image-responsive" src="" style="width: 170px;">
                                                    </div>
                                                @else
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;cursor: pointer;height: 100px;"
                                                         onclick="window.open('{{$popBackImage}}','_blank');">
                                                        <img class="image-responsive" src="{{$popBackImage}}"
                                                             style="width: 170px;">
                                                    </div>
                                                @endif
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.status')}}：</label>
                                    <div class="col-sm-6">
                                        <select class="form-select form-control"
                                                wire:model="modifyStatus" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.createdAt')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popCreatedAt" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.lastOperator')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOperator" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.lastUpdateTime')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUpdateAt" disabled>
                                    </div>
                                </div>
                                <div class="form-group" style="margin-bottom: 0;">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.remark')}}：</label>
                                    <div class="col-sm-8">
                                    <textarea rows="5" class="form-control"
                                              wire:model="modifyRemark" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}></textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div style="text-align: center;">
                                    <div class="btn btn-mint" wire:click="doModify('approved')">
                                        <i class="bi-check2-square"></i> {{__('web_message.general.approved')}}
                                    </div>
                                    <div class="btn btn-danger offset-1" wire:click="doModify('refused')">
                                        <i class="bi-x-square"></i> {{__('web_message.general.refused')}}
                                    </div>
                                </div>
                            </div>
                        </div>
                    @endif
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('#popClose').click(function () {
                    $('#idCardPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
