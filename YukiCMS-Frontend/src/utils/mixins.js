export default {
    computed: {
        loginUsr:function(){
            return this.$store.state.loginUsr;
        },
        enabledPages:function(){
            return this.$store.state.enabledPages;
        }
    },
    methods: {
        fetchGet:function(url){
            return fetch(this.$store.state.backendUrl+url, {
                method: 'GET',
                headers: new Headers({
                    'Authorization': this.$store.state.jwt,
                }),
            })
        },
        fetchPostJson:function(url,data){
            return fetch(this.$store.state.backendUrl+url, {
                method: 'POST',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Authorization': this.$store.state.jwt,
                    'Content-Type': 'application/json;charset=utf-8',
                }),
            })
        },
        fetchPutJson:function(url,data){
            return fetch(this.$store.state.backendUrl+url, {
                method: 'PUT',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Authorization': this.$store.state.jwt,
                    'Content-Type': 'application/json;charset=utf-8',
                }),
            })
        },
        fetchDelete:function(url){
            return fetch(this.$store.state.backendUrl+url, {
                method: 'DELETE',
                headers: new Headers({
                    'Authorization': this.$store.state.jwt,
                }),
            })
        },
        fetchUploadPost:function(url,file){
            const fd = new FormData();
            fd.append('file', file);
            return fetch(this.$store.state.backendUrl+url,{
                method: 'POST',
                body: fd,
                headers: new Headers({
                    'Authorization': this.$store.state.jwt,
                }),
            })
        },
    },
}