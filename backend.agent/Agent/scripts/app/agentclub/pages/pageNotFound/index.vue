<template>
   <v-app id="inspire">
    <div class="PageNotFound_content">
      <img src="../../assets/404.jpg"> 
    </div>
  </v-app>
</template>

<script>
export default {
  name: 'PageNotFound'
}
</script>

<style lang="scss" scoped>
  .PageNotFound_content{
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    background-color: #fff;
    font-size: 2em;
    font-weight: 900;
  }
</style>


