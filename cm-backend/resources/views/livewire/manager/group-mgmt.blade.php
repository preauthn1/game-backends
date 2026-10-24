@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.manager.groupMgmt')}}</h1>
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
                        <label>{{__('web_message.groupMgmt.groupType')}}：</label>
                        <select class="form-select form-control" wire:model="groupType" wire:change="changeGroupList">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                <option value="{{$data['id']}}">{{$data['name']}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.groupMgmt.groupName')}}：</label>
                        <select class="form-select form-control" wire:model="groupId">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['groupList']??[]) as $data)
                                <option value="{{$data['id']}}">{{$data['name']}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addAgentGroup">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}} {{__('web_message.groupMgmt.group')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.groupMgmt.mainDefine')}} ID</th>
                            <th>{{__('web_message.groupMgmt.mainDefine').' '.__('web_message.general.txt')}}</th>
                            <th>{{__('web_message.groupMgmt.subDefine')}} ID</th>
                            <th>{{__('web_message.groupMgmt.subDefine').' '.__('web_message.general.txt')}}</th>
                            <th>{{__('web_message.general.dataType')}}</th>
                            <th>{{__('web_message.general.content')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $mainIndex=>$mainData)
                            @forelse(($mainData->sub??[]) as $subIndex=>$subData)
                                @if($subIndex==0)
                                    <tr class="{{(0==$mainIndex%2)?'main-define-even':'main-define-odd'}}">
                                        <td>
                                            <button class="btn btn-warning"
                                                    wire:click="showModifySetting({{$subData->id??''}})">
                                                <i class="bi-pencil-square"></i>
                                            </button>
                                        </td>
                                        <td>{{$mainData->mainId}}</td>
                                        <td>{{$mainData->mainName}}</td>
                                        <td>{{$subData->subId}}</td>
                                        <td>{{$subData->subName}}</td>
                                        <td>{{$subData->dataTypeTxt}}</td>
                                        <td>{{$subData->value}}</td>
                                    </tr>
                                @else
                                    <tr>
                                        <td>
                                            <button class="btn btn-warning"
                                                    wire:click="showModifySetting({{$subData->id??''}})">
                                                <i class="bi-pencil-square"></i>
                                            </button>
                                        </td>
                                        <td></td>
                                        <td></td>
                                        <td>{{$subData->subId}}</td>
                                        <td>{{$subData->subName}}</td>
                                        <td>{{$subData->dataTypeTxt}}</td>
                                        <td>{{$subData->value}}</td>
                                    </tr>
                                @endif
                            @empty
                            @endforelse
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
                    <style>
                        tr.main-define-odd td {
                            background-color: rgba(22, 174, 245, 0.5);
                        }

                        tr.main-define-even td {
                            background-color: rgba(22, 174, 245, 0.3);
                        }
                    </style>
                </div>
            </div>
        </div>
        <!-- addAgentGroup -->
        <div class="modal fade" id="addAgentGroup" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.general.add')}} {{__('web_message.groupMgmt.group')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addAgentGroup">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.groupMgmt.groupType')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addAgentGroupType">
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                                <option value="{{$data['id']}}">{{$data['name']}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.groupMgmt.groupName')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="addAgentGroupName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-9">
                                        <input type="text" class="form-control" wire:model="addAgentGroupMemo">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- modifySetting -->
        <div class="modal fade in" id="modifySetting" role="dialog"
             style="display: {{'modifySetting' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifySettingClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.general.content').' '.__('web_message.general.modify')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModifySetting">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifySettingId">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}} ID
                                    </label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="popMainId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine').' '.__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="popMainName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.subDefine')}} ID
                                    </label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="popSubId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.subDefine').' '.__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="popSubName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.dataType')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="popDataTypeTxt" disabled>
                                    </div>
                                </div>
                                @if('boolean'==$popDataType)
                                    <div class="form-group">
                                        <label class="col-sm-3 control-label">
                                            {{__('web_message.general.content')}}
                                        </label>
                                        <div class="col-sm-8">
                                            <input id="modify-menu-isShow" class="toggle-switch" type="checkbox"
                                                   wire:model="modifySettingValue">
                                            <label for="modify-menu-isShow"></label>
                                        </div>
                                    </div>
                                @elseif('integer'==$popDataType)
                                    <div class="form-group">
                                        <label class="col-sm-3 control-label">
                                            {{__('web_message.general.content')}}
                                        </label>
                                        <div class="col-sm-8">
                                            <input type="text" class="form-control" wire:model="modifySettingValue"
                                                   oninput="value=value.replace(/\D/g,'')">
                                        </div>
                                    </div>
                                @elseif('decimal'==$popDataType)
                                    <div class="form-group">
                                        <label class="col-sm-3 control-label">
                                            {{__('web_message.general.content')}}
                                        </label>
                                        <div class="col-sm-8">
                                            <input type="text" class="form-control" wire:model="modifySettingValue"
                                                   oninput="value=value.replace(/[^0-9.]/g,'').replace(/^(\d*\.\d*)\.+/g, '$1')">
                                        </div>
                                    </div>
                                @else
                                    <div class="form-group">
                                        <label class="col-sm-3 control-label">
                                            {{__('web_message.general.content')}}
                                        </label>
                                        <div class="col-sm-8">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="modifySettingValue"></textarea>
                                        </div>
                                    </div>
                                @endif
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="popMemo" disabled></textarea>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#modifySettingClose').click(function () {
                    $('#modifySetting').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
