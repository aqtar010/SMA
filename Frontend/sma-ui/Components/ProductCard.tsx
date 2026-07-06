import { ProductResponseDto } from "@/DTOs/ProductDTOs";

export default function ProductCard({name,price,description,quantityAvailable,id}:ProductResponseDto) {

    return (

        <div className="border-b-mist-950 rounded-3xl p-4 ms-2" 
        style={{
            background:quantityAvailable>0? 'green':'none'
        }}
        >
            <p>{id}</p>
            <p>{name}</p>
            <p>${price}</p>
            <p>{description}</p>
        </div>
    );
}