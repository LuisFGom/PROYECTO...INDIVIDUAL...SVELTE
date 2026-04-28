<script>
  import { closeModal, setModalLoading } from '../stores/store.js'
  import Swal from 'sweetalert2'

  export let isOpen = false
  export let title = ''
  export let content = null
  export let onConfirm = null
  export let isLoading = false

  const handleClose = () => {
    closeModal()
  }

  const handleConfirm = async () => {
    if (onConfirm) {
      setModalLoading(true)
      try {
        await onConfirm()
      } catch (error) {
        await Swal.fire({
          icon: 'error',
          title: 'Error',
          text: error.message || 'Ocurrió un error'
        })
      } finally {
        setModalLoading(false)
      }
    }
  }
</script>

{#if isOpen}
  <div class="modal-overlay" role="button" tabindex="0" on:click={handleClose} on:keydown={(e) => { if (e.key === 'Escape') handleClose(); }}>
    <div class="modal-content" on:click|stopPropagation>
      <div class="modal-header">
        <h2 class="modal-title">{title}</h2>
        <button class="modal-close" on:click={handleClose} disabled={isLoading}>
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="modal-body">
        <svelte:component this={content} />
      </div>
      <div class="modal-footer">
        <button class="btn btn-secondary" on:click={handleClose} disabled={isLoading}>
          Cancelar
        </button>
        <button class="btn btn-primary" on:click={handleConfirm} disabled={isLoading}>
          {#if isLoading}
            <i class="fas fa-spinner fa-spin"></i>
          {/if}
          Confirmar
        </button>
      </div>
    </div>
  </div>
{/if}

<style>
  :global(.modal-overlay) {
    animation: fadeIn 0.2s ease-in-out;
  }

  :global(.modal-content) {
    animation: slideUp 0.3s ease-out;
  }

  @keyframes fadeIn {
    from {
      opacity: 0;
    }
    to {
      opacity: 1;
    }
  }

  @keyframes slideUp {
    from {
      transform: translateY(20px);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }
</style>
