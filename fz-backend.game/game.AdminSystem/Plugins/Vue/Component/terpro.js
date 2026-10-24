Vue.component('vItem',
   {
       template: `
                <div>
                    <div class ="v_item__contain" v-for="(no , index) in list" v-bind:class ="{ 'js--active': no.ID == active}" v-on:click="choose(no.ID)">
                        <div>{{no.Name}}</div>
                    </div>
                </div>`,
       props: {
           list: Array,
           active: [String, Number]
       },

       methods: {
           choose: function (id) {
               this.$emit('chooseid', id)
           }
       }
   })


Vue.component('vList',
   {
       template: `
             <div class ="v_list">
                <div class ="v_list_headers">
                    <div v-for="(no , index) in headers">
                        {{no.text}}
                    </div>
                    <div v-if="!read"></div>
                </div>
                <div class ="v_list_items">
                    <div v-for="(no , index) in items" class ="v_list_row">
                        <div v-for="(k , index) in headers" >
                            <a v-if="read && k.key === 'UserID'" class ="link" target="_blank"
                            v-bind:href="'/UserInfo/Search?userId=' + getVal(k.key, no) ">{{getVal(k.key, no) }}</a>
                            <label v-else-if="read">{{ getVal(k.key, no) }}</label>
                            <input v-else class ="form-control input-small"
                            v-bind:id="k.key"
                            v-bind:data-row="no.ID"
                            v-bind:data-uuid="no._uuid"
                            v-bind:value="getVal(k.key, no)"
                            @change="rewrite($event)"/>
                        </div>
                        <div v-if="!read" v-on:click="deleteItem(no)">
                            <i class ="fas fa-minus-square fa-3x"></i>
                        </div>
                    </div>
                </div>
                <slot></slot>
            </div>`,
       props: {
           headers: Array,
           items: Array,
           read: Boolean
       },
       methods: {
           getVal(key, object) {
               const temp = object.items[key]
               if (temp !== void 0) return temp
               return void 0
           },
           rewrite (eve) {
               const deep = _.clone(this.items)
               const id = eve.target.id
               const val = eve.target.value
               const row = eve.target.dataset.row
               const uuid = eve.target.dataset.uuid
               const item = deep.map((x) => {
                   if (x._uuid !== void 0 && x._uuid === uuid) x.items[id] = val
                   if (x.ID !== null && x.ID == row) x.items[id] = val
                   return x
               })
               this.$emit('update:items', item)
           },
           deleteItem (i) {
               const id = _.clone(i.ID)
               const deep = _.clone(this.items)
               const uuid = _.clone(i._uuid)
               let temp = []

               if (_.isNull(id)) {
                   temp = deep.filter((d) =>  d._uuid !== uuid)
               } else {
                   temp = deep.filter((d) =>  d.ID !== id)
                   this.$emit('delete-item', id)
               }
               this.$emit('update:items', temp)
           }
       }
   })

Vue.component('vInput',
   {
       template: `
                <div class ="v_form__group">
                    <div class ="form_group__label">
                        {{label}}
                        <i v-if="required" class ="fas fa-star-of-life group_label__important"></i>
                    </div>
                    <div class ="form_group__div">
                        <input v-if="type === 'text'" type="text" class ="form-control input-medium"
                        v-model="val" />

                        <input v-if="type === 'number'" type="number" min="0" class ="form-control input-medium"
                        v-model="val" />

                        <input v-if="type === 'datetime'"  type="text" v-bind:id="id" class ="form-control input-medium "
                        onfocus="WdatePicker({ dateFmt: 'yyyy-MM-dd HH:mm', isShowClear: false})"
                        v-bind:placeholder="placeholder"
                        v-model="val">

                        <input v-if="type === 'time'"  type="text" v-bind:id="id" class ="form-control input-medium "
                        onfocus="WdatePicker({ dateFmt: 'HH:mm', isShowClear: false})"
                        v-bind:placeholder="placeholder"
                        v-model="val">

                        <select v-if="type === 'select'" class ="form-control input-medium" v-model="val">
                            <option v-for="item in items"
                                    v-bind:value="item.value"
                                    v-bind:selected="val == item.value">
                                    {{ item.text }}
                            </option>
                        </select>
                    </div>
                </div>`,
       data () {
           return {
               val: ''
           }
       },
       model: {
           prop: 'value',
           event: 'update'
       },
       props: {
           required: Boolean,
           id: [String, Number],
           label: [String, Number],
           value: [String, Number],
           items: Array,
           placeholder: [String, Number],
           type: {
               default: 'text',
               type: String
           }
       },
       watch: {
           value (val) {
               this.$set(this, 'val', val)
           },
           val(val) {
               this.$emit('update', val)
           }
       },
       methods: {
       }
   })


Vue.component('vLoading',
   {
       template: `  <div class="v_loading_contain">
                        <div class ="lds-facebook"><div></div><div></div><div></div></div>
                    </div>`,
   })
