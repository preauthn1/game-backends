@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.taskScheduling')}}</h1>
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
                    <label>{{__('web_message.taskScheduling.command')}}：</label>
                    <input type="text" class="form-control" wire:model="command">
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.status')}}：</label>
                    <select class="form-select form-control" wire:model="taskStatus">
                        <option value="-1">{{__('web_message.general.select')}}</option>
                        @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                        <option value="{{$data->id}}">{{$data->name}}</option>
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
                        <button class="btn btn-purple" data-toggle="modal" data-target="#addTask">
                            <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                        </button>
                    </div>
                </div>
            </div>
            <div class="table-responsive">
                <table class="table txt-center">
                    <thead>
                        <tr>
                            <th></th>
                            <th>ID</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.taskScheduling.command')}}</th>
                            <th>{{__('web_message.taskScheduling.expression')}}<br />({{__('web_message.taskScheduling.expressionTip')}})</th>
                            <th>{{__('web_message.taskScheduling.description')}}</th>
                            <th>{{__('web_message.taskScheduling.lastRunTime')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                        <tr>
                            <td>
                                <button class="btn btn-warning" wire:click="showModify({{$data->id}})">
                                    <i class="bi-pencil-square"></i>
                                </button>
                            </td>
                            <td>{{ $data->id }}</td>
                            <td style="font-size: 16px;"><i
                                    class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i></td>
                            <td>{{ $data->command }}</td>
                            <td>{{ $data->expression }}</td>
                            <td>{{ $data->description }}</td>
                            <td>{{ $data->last_run_at }}</td>
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
    <!-- addTask -->
    <div class="modal fade" id="addTask" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">{{__('web_message.menu.system.addTask')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addTask">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.taskScheduling.command')}}
                                </label>
                                <div class="col-sm-7">
                                    <input type="text" class="form-control" wire:model="addTaskCommand">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.taskScheduling.expression')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addTaskExpression" style="font-size: 16px;">
                                    <div>{{__('web_message.taskScheduling.expressionTip')}}</div>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.description')}}</label>
                                <div class="col-sm-7">
                                    <textarea rows="3" class="form-control"
                                        wire:model="addTaskDescription"></textarea>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.status')}}
                                </label>
                                <div class="col-sm-5 lh-30">
                                    <input id="add-task-status" class="toggle-switch" type="checkbox"
                                        wire:model="addTaskStatus">
                                    <label for="add-task-status"></label>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.otherSetting')}}</label>
                                <div class="col-sm-7">
                                    <textarea rows="3" class="form-control"
                                        wire:model="addTaskInfo"></textarea>
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
    <!-- modifyTask -->
    <div class="modal fade in" id="modifyTask" role="dialog"
        style="display: {{'modifyTask' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="modifyClose">&times;</button>
                    <h4 class="modal-title">{{__('web_message.menu.system.modifyTask')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="doModify">
                        <div class="panel-body">
                            <input type="hidden" wire:model="popTaskId">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.command')}}</label>
                                <div class="col-sm-7">
                                    <input type="text" class="form-control" wire:model="popTaskCommand" disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.expression')}}</label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyTaskExpression" style="font-size: 16px;">
                                    <div>{{__('web_message.taskScheduling.expressionTip')}}</div>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.description')}}</label>
                                <div class="col-sm-7">
                                    <textarea rows="3" class="form-control"
                                        wire:model="modifyTaskDescription"></textarea>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.general.status')}}</label>
                                <div class="col-sm-5 lh-30">
                                    <input id="modify-task-status" class="toggle-switch" type="checkbox"
                                        wire:model="modifyTaskStatus">
                                    <label for="modify-task-status"></label>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.taskScheduling.otherSetting')}}</label>
                                <div class="col-sm-7">
                                    <textarea rows="3" class="form-control"
                                        wire:model="modifyTaskInfo"></textarea>
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
        $(function() {
            $('#modifyClose').click(function() {
                $('#modifyTask').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
        });
    </script>
</div>
@endsection