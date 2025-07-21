using UnityEngine;

public class PotItem : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] private float itemWeight = 0.5f;
    [SerializeField] private bool destroyOnAdd = false;
    [SerializeField] private float destroyDelay = 1f;
    
    [Header("Respawn")]
    [SerializeField] private bool enableRespawn = true;
    [SerializeField] private float respawnHeight = -9f;
    [SerializeField] private Vector3 respawnOffset = new Vector3(0, 2f, 0);
    [SerializeField] private bool useCustomRespawnPoint = false;
    [SerializeField] private Vector3 customRespawnPoint = Vector3.zero;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool hasBeenAdded = false;
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb && debugMode)
        {
            Debug.LogWarning($"{gameObject.name} n'a pas de Rigidbody!");
        }
        
        // Sauvegarder la position initiale pour le respawn
        // Utilise Invoke pour s'assurer que tous les scripts ont fini leur initialisation
        Invoke(nameof(SaveInitialPosition), 0.1f);
    }
    
    void SaveInitialPosition()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        if (debugMode)
            Debug.Log($"Position de respawn sauvegardée : {initialPosition}");
    }
    
    void Update()
    {
        // Vérifier si l'objet est tombé trop bas
        if (enableRespawn && transform.position.y < respawnHeight)
        {
            Respawn();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Éviter d'ajouter plusieurs fois
        if (hasBeenAdded) return;
        
        // Vérifier si c'est le trigger du pot
        if (other.CompareTag("PotTrigger") || other.name.Contains("PotContainer"))
        {
            if (debugMode)
                Debug.Log($"{gameObject.name} entre dans le pot!");
            
            // Trouver le script de la balance
            BalanceController balance = FindObjectOfType<BalanceController>();
            
            if (balance)
            {
                // Ajouter le poids à la balance
                balance.AddWeight(itemWeight);
                hasBeenAdded = true;
                
                if (debugMode)
                    Debug.Log($"Poids de {itemWeight}kg ajouté à la balance");
                
                // Gérer l'objet après ajout
                HandleAfterAdd();
            }
            else
            {
                Debug.LogError("BalanceController introuvable!");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        
    }
    
    private void HandleAfterAdd()
    {
        // Désactiver la physique pour éviter que l'objet bouge
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Optionnel : rendre l'objet enfant du pot pour qu'il suive
        Transform potTransform = GameObject.Find("PotContainer")?.transform;
        if (potTransform)
        {
            transform.SetParent(potTransform);
        }
        
        // Désactiver le respawn une fois dans le pot
        enableRespawn = false;
        
        // Toujours détruire l'objet après l'avoir ajouté au pot
        // Le poids reste dans la balance même si l'objet est détruit
        Destroy(gameObject, destroyDelay);
    }
    
    private void Respawn()
    {
        if (debugMode)
            Debug.Log($"{gameObject.name} est tombé ! Respawn...");
        
        // Si l'objet était dans le pot, retirer son poids
        if (hasBeenAdded)
        {
            BalanceController balance = FindObjectOfType<BalanceController>();
            if (balance)
            {
                balance.RemoveWeight(itemWeight);
            }
            hasBeenAdded = false;
        }
        
        // Utiliser la position custom si activée, sinon la position initiale
        Vector3 respawnPosition = useCustomRespawnPoint ? customRespawnPoint : initialPosition;
        
        // Réinitialiser la position et rotation
        transform.position = respawnPosition + respawnOffset;
        transform.rotation = initialRotation;
        
        // Réinitialiser la physique
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
    }
    
    void OnDestroy()
    {
        // NE PAS retirer le poids quand l'objet est détruit après être tombé dans le pot
        // Le poids doit rester dans la balance
        if (hasBeenAdded && debugMode)
        {
            Debug.Log($"{gameObject.name} détruit, mais le poids reste dans la balance");
        }
    }
    
    // Méthode publique pour changer le poids
    public void SetWeight(float newWeight)
    {
        // Si déjà dans le pot, mettre à jour la balance
        if (hasBeenAdded)
        {
            BalanceController balance = FindObjectOfType<BalanceController>();
            if (balance)
            {
                balance.RemoveWeight(itemWeight);
                itemWeight = newWeight;
                balance.AddWeight(itemWeight);
            }
        }
        else
        {
            itemWeight = newWeight;
        }
    }
    
    // Méthode publique pour définir le point de respawn
    public void SetRespawnPoint(Vector3 newPosition)
    {
        initialPosition = newPosition;
    }
    
    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = hasBeenAdded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            
            // Afficher le poids
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, $"{itemWeight}kg");
            #endif
            
            // Dessiner la ligne de respawn
            if (enableRespawn)
            {
                Gizmos.color = Color.red;
                Vector3 lineStart = new Vector3(-50, respawnHeight, 0);
                Vector3 lineEnd = new Vector3(50, respawnHeight, 0);
                Gizmos.DrawLine(lineStart, lineEnd);
            }
        }
    }
}